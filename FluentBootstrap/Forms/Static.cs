﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentBootstrap.Forms
{
    public class Static<TModel> : FormControl<TModel>
    {
        internal Static(BootstrapHelper<TModel> helper)
            : base(helper, "p", "form-control-static")
        {
        }
    }
}
