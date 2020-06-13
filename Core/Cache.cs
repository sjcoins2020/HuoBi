using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jli.Core
{

    /// <summary>
    /// 读取标志类
    /// </summary>
    public class ReadFlag
    {
        private int readCount;
        /// <summary>
        /// 读取次数
        /// </summary>
        public int ReadCount
        {
            get { return readCount; }
            set { readCount = value; }
        }
        private DateTime lastReadTime;
        /// <summary>
        /// 最后读取时间
        /// </summary>
        public DateTime LastReadTime
        {
            get { return lastReadTime; }
            set { lastReadTime = value; }
        }
        public ReadFlag(DateTime birthTime)
        {
            this.readCount = 0;
            this.lastReadTime = birthTime;
        }
    }
    public class ElementBuffer<TKey, Tobject>
    {
        public Dictionary<TKey, ReadFlag> ReadFlagBuffer = new Dictionary<TKey, ReadFlag>();
        public Dictionary<TKey, Tobject> Buffer = new Dictionary<TKey, Tobject>();
    }
    /// <summary>
    /// 对象缓存器
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="Tobject"></typeparam>
    public class ObjectCache<TKey, Tobject>
    {
        private int objectCount = 100;
        /// <summary>
        /// 缓存内最大允许的Object数量
        /// </summary>
        public int ObjectCount
        {
            get { return objectCount; }
            set { objectCount = value; }
        }
        private int loseTime = 60;
        /// <summary>
        /// 失效时间，秒为单位
        /// </summary>
        public int LoseTime
        {
            get { return loseTime; }
            set { loseTime = value; }
        }
        private ElementBuffer<TKey, Tobject> buffer = new ElementBuffer<TKey, Tobject>();
        public object this[TKey key]
        {
            get
            {
                Tobject ret;
                if (this.buffer.ReadFlagBuffer.ContainsKey(key) && this.buffer.Buffer.TryGetValue(key, out ret))
                {
                    lock (this.buffer)
                    {
                        this.buffer.ReadFlagBuffer[key].ReadCount++;
                        this.buffer.ReadFlagBuffer[key].LastReadTime = DateTime.Now;
                    }
                    return ret;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                Tobject ret;
                if (this.buffer.ReadFlagBuffer.ContainsKey(key) && this.buffer.Buffer.TryGetValue(key, out ret))
                {
                    ret = (Tobject)value;
                }
                else
                {
                    lock (this.buffer)
                    {
                        if (this.buffer.ReadFlagBuffer.Count > this.objectCount)
                        {
                            TKey removeKey;
                            TKey[] array = new TKey[this.buffer.ReadFlagBuffer.Keys.Count];
                            this.buffer.ReadFlagBuffer.Keys.CopyTo(array, 0);
                            removeKey = array[0];
                            foreach (TKey tkey in this.buffer.ReadFlagBuffer.Keys)
                            {
                                if (this.buffer.ReadFlagBuffer[tkey].LastReadTime.AddSeconds(this.LoseTime).CompareTo(DateTime.Now) < 0)
                                {
                                    removeKey = tkey;
                                    break;
                                }
                                if (this.buffer.ReadFlagBuffer[tkey].ReadCount < this.buffer.ReadFlagBuffer[removeKey].ReadCount)
                                {
                                    removeKey = tkey;
                                }
                            }
                            this.Remove(removeKey);
                        }
                        else
                        {
                            ReadFlag readFlag = new ReadFlag(DateTime.Now);
                            this.buffer.ReadFlagBuffer.Add(key, readFlag);
                            this.buffer.Buffer.Add(key, (Tobject)value);
                        }
                    }
                }
            }
        }
        private void Remove(TKey key)
        {
            lock (this.buffer)
            {
                this.buffer.Buffer.Remove(key);
                this.buffer.ReadFlagBuffer.Remove(key);
            }
        }
    }
}
