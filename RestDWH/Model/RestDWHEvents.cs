using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RestDWH.Model
{
    public class RestDWHEvents<T>
        where T : class
    {
        public virtual async Task<(int from, int size, string query, string sort)> BeforeGetAsync(int from = 0, int size = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null)
        {
            return (from, size, query, sort);
        }
        public virtual async Task<DBListBase<T, DBBase<T>>> AfterGetAsync(DBListBase<T, DBBase<T>> result, int from = 0, int size = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null)
        {
            return result;
        }

        public virtual async Task<string> BeforeGetByIdAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            return id;
        }
        public virtual async Task<DBBase<T>> AfterGetByIdAsync(DBBase<T> result, string id, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            return result;
        }
    }
}
