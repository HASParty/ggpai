using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools {
    public static class Stringify<T> {
        public static string List(List<T> l, string sep = " ") {
            string ret = "";
            foreach(T item in l) {
                ret += item.ToString()+sep;
            }
            return ret;
        }

        public static string Array(T[] l, string sep = " ") {
            string ret = "";
            foreach(T item in l) {
                ret += item.ToString()+sep;
            }
            return ret;
        }
    }
}
