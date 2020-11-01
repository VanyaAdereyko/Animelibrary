using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Animelibrary.Models
{
    public class Index
    {
        public string Time { get; set; }
        public void OnGet()
        {
            Time = DateTime.Now.ToShortTimeString();
        }
    }
}
