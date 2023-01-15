using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        private static class ErrorState
        { // ErrorState
            public enum Type
            {
                TooManyControllers, NoRemoteController
            };
            private static Dictionary<Type, bool> errorState = new Dictionary<Type, bool> {
                { Type.TooManyControllers, false }
            };
            public static void Set(Type type)
            {
                errorState[type] = true;
            }

            public static void Reset(Type type)
            {
                errorState[type] = false;
            }

            public static bool Get(Type type)
            {
                if (!errorState.ContainsKey(type))
                {
                    return false;
                }
                return errorState[type];
            }
        }
    }
}
