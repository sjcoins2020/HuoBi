using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jli.Core
{
    /// <summary>  
    /// 一个类似于自旋锁的类，也类似于对共享资源的访问机制  
    /// 如果资源已被占有，则等待一段时间再尝试访问，如此循环，直到能够获得资源的使用权为止  
    /// </summary>  
    public class SpinLock
    {
        //资源状态锁，0--未被占有， 1--已被占有  
        private int theLock = 0;
        //等待时间  
        private int spinWait;

        public SpinLock(int spinWait)
        {
            this.spinWait = spinWait;
        }

        /// <summary>  
        /// 访问  
        /// </summary>  
        public void Enter()
        {
            //如果已被占有，则继续等待  
            while (Interlocked.CompareExchange(ref theLock, 1, 0) == 1)
            {
                ///Thread.Sleep(spinWait);
                Delay(spinWait);
            }
        }

        public  void Delay(int mm)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }

        /// <summary>  
        /// 退出  
        /// </summary>  
        public void Exit()
        {
            //重置资源锁  
            Interlocked.Exchange(ref theLock, 0);
        }
    }

    /// <summary>  
    /// 自旋锁的管理类   
    /// </summary>  
    public class SpinLockManager : IDisposable  //Disposable接口,实现一种非委托资源回收机制，可看作显示回收资源。任务执行完毕后，会自动调用Dispose()里面的方法。  
    {
        private SpinLock spinLock;

        public SpinLockManager(SpinLock spinLock)
        {
            this.spinLock = spinLock;
            spinLock.Enter();
        }

        //任务结束后，执行Dispose()里面的方法  
        public void Dispose()
        {
            spinLock.Exit();
        }
    }

}
