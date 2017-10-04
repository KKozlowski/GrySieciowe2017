using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Net
{
    public class Dbg
    {
        public static void Log( string msg )
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log( msg );
#endif
        }

        public static void Assert( bool cond, string msg )
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Assert( cond, msg );
#endif
        }

        public static void Assert( bool cond )
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Assert( cond );
#endif
        }
    }
}