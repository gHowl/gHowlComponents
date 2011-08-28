using System;
using System.Collections.Generic;
using System.Text;

namespace gHowl.Udp
{
   
    enum SendPattern
    { 
        Text = 0,
        DoubleArray = 10,

#if WITH_OSC
        OSC = 999
#endif
    }
    
}
