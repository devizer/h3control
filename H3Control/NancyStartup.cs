namespace H3Control
{
    using Owin;

    public class NancyStartup
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

            app.Use(typeof (WhiteListMiddleware), WhiteListConfig.WhiteList);
            app.UseNancy();
        }

    }
}