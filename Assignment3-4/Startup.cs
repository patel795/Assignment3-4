﻿using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Assignment3_4.Startup))]
namespace Assignment3_4
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
