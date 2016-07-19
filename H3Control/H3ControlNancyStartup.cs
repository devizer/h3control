namespace H3Control
{
    using System;

    using Common;

    using Owin;

    using Universe;

    public class H3ControlNancyStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // app.UseWelcomePage("/owin");

            try
            {
                DeviceDataSource.GetLocal();
            }
            catch
            {
            }

            Action<string> trace = dump =>
                FirstRound.Only("OWIN request dump", RoundCounter.Twice, () =>
                {
                    NiceTrace.Message(dump);
                });


            app.Use(typeof (TraceMiddleware), trace);
            app.Use(typeof (WhiteListMiddleware), H3WhiteListConfig.WhiteList);
            app.UseNancy();
        }

    }
}