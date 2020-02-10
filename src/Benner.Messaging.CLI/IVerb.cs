using System;
using System.Collections.Generic;
using System.Text;

namespace Benner.Messaging.CLI
{
    public interface IVerb
    {
        void Configure();
        bool HasNoInformedParams();
    }
}
