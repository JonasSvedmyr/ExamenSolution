using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Services
{
    public interface ILogger
    {
        public ILog GetLogger(Type type);//Behövs denna eller funkar den generiska att använda sig av
        public ILog GetLogger<T>();
    }
}
