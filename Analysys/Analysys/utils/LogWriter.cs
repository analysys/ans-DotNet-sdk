using System;
using System.IO;
using System.Threading;

namespace Analysys.utils {
    public static class LogWriter {
        /// <summary>
        /// 将一个字符串写入到一个文件
        /// </summary>
        /// <param name="fileName">写入的文件名</param>
        /// <param name="value">写入的内容</param>
        /// <param name="lockName">多进程写场景下，要锁的文件</param>
        /// <param name="appEnd">是否追加</param>
        public static bool Writer(string dir, string fileName, string value, string lockName, bool appEnd) {
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(value)) {
                return false;
            }

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            var filePath = dir + Path.DirectorySeparatorChar + fileName;

            LockProcess(lockName);

            StreamWriter writer = new StreamWriter(filePath, appEnd);

            writer.WriteLine(value);
            writer.Flush();
            writer.Close();

            ReleaseProcess(lockName);

            return true;
        }

        private static void LockProcess(string lockFile) {
            try {
                if (string.IsNullOrEmpty(lockFile)) return;
                Mutex mutex = null;
                try {
                    mutex = Mutex.OpenExisting(lockFile);
                }
                catch (Exception e) {
                    mutex = new Mutex(false, lockFile);
                }

                mutex?.WaitOne();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private static void ReleaseProcess(string lockFile) {
            try {
                if (string.IsNullOrEmpty(lockFile)) return;
                Mutex mutex = null;
                try {
                    mutex = Mutex.OpenExisting(lockFile);
                }
                catch (Exception e) {
                    mutex = new Mutex(true, lockFile);
                }

                mutex?.ReleaseMutex();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}