using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Analysys.utils;

namespace Analysys
{
    public class LogCollecter : ICollecter
    {
        private const string linefeed = "\n";
        private readonly string _logFolder;
        private readonly bool _async = false;
        private readonly bool singleObj = true;
        private string format;
        private const int RETRY_TIMES = 3; //重试3次
        private const int DEFAULT_BATCH_NUM = 20;
        private const long DEFAULT_BATCH_SEC = 10;
        private long sendTimer = -1;
        private List<IDictionary<string, object>> batchMsgList;
        private int batchNum;
        private long batchSec;
        private bool debug;
        private System.Timers.Timer timer;

        public LogCollecter(string logFolder) : this(logFolder, GeneralRule.HOUR, false, DEFAULT_BATCH_NUM,
            DEFAULT_BATCH_SEC, true)
        {
        }

        public LogCollecter(string logFolder, bool async) : this(logFolder, GeneralRule.HOUR, async, DEFAULT_BATCH_NUM,
            DEFAULT_BATCH_SEC, true)
        {
        }

        public LogCollecter(string logFolder, GeneralRule rule) : this(logFolder, rule, false, DEFAULT_BATCH_NUM,
            DEFAULT_BATCH_SEC, true)
        {
        }

        public LogCollecter(string logFolder, GeneralRule rule, bool async) : this(logFolder, rule, async,
            DEFAULT_BATCH_NUM, DEFAULT_BATCH_SEC, true)
        {
        }

        public LogCollecter(string logFolder, GeneralRule rule, bool async, int batchNum, long batchSec) : this(
            logFolder, rule, async, batchNum, batchSec, true)
        {
        }

        private LogCollecter(string logFolder, GeneralRule rule, bool async, int batchNum, long batchSec,
            bool singleObj)
        {
            if (string.IsNullOrEmpty(logFolder))
            {
                throw new Exception("logFolder is empty");
            }

            this._logFolder = logFolder;
            if (!Directory.Exists(logFolder) || File.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            string @lock = "lock";
            if (GeneralRule.DAY.Equals(rule))
            {
                this.format = "yyyyMMdd";
                @lock = @lock + "_day";
            }
            else
            {
                this.format = "yyyyMMddHH";
                @lock = @lock + "_hour";
            }

            this._async = async;
            this.singleObj = singleObj;
            this.batchMsgList = new List<IDictionary<string, object>>(1);
            if (this._async)
            {
                this.batchNum = batchNum;
                this.batchSec = batchSec * 1000;
                this.batchMsgList = new List<IDictionary<string, object>>(this.batchNum);

                Init();
            }
        }

        private void Init()
        {
            timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000; //执行间隔时间,单位为毫秒; 这里实际间隔为1秒钟  
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(task);
        }

        private void task(object source, ElapsedEventArgs eea)
        {
            if (sendTimer == -1 || (TimeHelper.CurrentTimeMillis() - sendTimer < batchSec)) return;
            try
            {
                Upload();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private void dealLog(IList<IDictionary<string, object>> batchMsgList)
        {
            bool success = false;
            if (singleObj)
            {
                StringBuilder sb = new StringBuilder();
                int index = 0;
                foreach (IDictionary<string, object> map in batchMsgList)
                {
                    string jsonData = JsonHelper.Serialize(map);
                    if (++index > 1)
                    {
                        sb.Append(linefeed);
                    }

                    sb.Append(jsonData);
                }

                success = dealLog(sb.ToString());
            }
            else
            {
                string jsonData = JsonHelper.Serialize(batchMsgList);
                success = dealLog(jsonData);
            }

            if (!success)
            {
                Console.WriteLine("Error After Retry " + RETRY_TIMES + " Times: " + JsonHelper.Serialize(batchMsgList));
            }
        }

        private bool dealLog(string jsonData)
        {
            var time = DateTime.Now.ToString(format);
            var name = "datas_" + time + ".log";
            var lockName = "analysys_pc_lock_" + time;
            bool success = LogWriter.Writer(_logFolder, name, jsonData, lockName, true);
            if (!success)
            {
                int total = RETRY_TIMES;
                while (!success && total-- > 0)
                {
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1);
                    }

                    success = LogWriter.Writer(_logFolder, name, jsonData, lockName, true);
                }
            }

            return success;
        }


        public bool Send(Dictionary<string, object> egCollectMessage)
        {
            try
            {
                if (!_async)
                {
                    IList<IDictionary<string, object>> egMsgList = new List<IDictionary<string, object>>();
                    egMsgList.Add(egCollectMessage);
                    dealLog(egMsgList);
                }
                else
                {
                    lock (batchMsgList)
                    {
                        if (sendTimer == -1)
                        {
                            sendTimer = TimeHelper.CurrentTimeMillis();
                        }

                        batchMsgList.Add(egCollectMessage);
                    }

                    if (batchMsgList.Count >= batchNum)
                    {
                        Upload();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                try
                {
                    Console.WriteLine("Log Data Error: " + JsonHelper.Serialize(egCollectMessage));
                }
                catch (Exception)
                {
                }

                return false;
            }
        }

        public void Upload()
        {
            lock (batchMsgList)
            {
                if (batchMsgList != null && batchMsgList.Count > 0)
                {
                    try
                    {
                        dealLog(batchMsgList);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Json Serialize Error: " + e);
                    }
                    finally
                    {
                        batchMsgList.Clear();
                        if (this._async)
                        {
                            resetTimer();
                        }
                    }
                }
            }
        }

        private void resetTimer()
        {
            this.sendTimer = -1;
        }

        public void Flush()
        {
            Upload();
        }

        public void Close()
        {
            //保存内存里面的数据
            Flush();
            //关闭定时器
            try
            {
                timer.Stop();
                timer.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Debug(bool isDebugModel)
        {
            this.debug = isDebugModel;
        }
    }
}