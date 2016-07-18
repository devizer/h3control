namespace Universe.NancyCaching
{
    using System;
    using System.Globalization;

    using Nancy;
    using Nancy.Bootstrapper;

    //  Bootstrapper: pipelines.BindExpiration();
    // +~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~+
    // this.Expires()
    //      �� ���������� �� � �������� �� � ������. ���� HTTP/1.0, ���� HTTP/1.1
    // this.Expires(timeSpan) - 
    //      ���������� � � �������� � � ������ �������� �����.
    //      ���� � ������ � ������� ������������ HTTP/1.1, �� �������� ������ ������������ ����� �������������� �� ������ � ���������.
    //      ���� ��� ������ ��� ������� ������������ ������ HTTP/1.0, �� (Now+timeSpan) ����������� �� �������, �� ������������ �������������� �� ������ � ��������. ��� ����� HTTP/1.0 - � ���� ������ ����� ������� ������ ��� ������
    // this.Expires(timeSpan, scope: CachingScope.Private)
    //      ���� ��� ������ ��� ������� ������������ ������ HTTP/1.0, �� ���� ���� ��� ������ �������
    //      ���� � ������ � ������� ������������ HTTP/1.1, �� �� ������ �� ����������, � � �������� ���������� �������� �����. ������ ����� ���� ����������.
    // 
    // ��������� �������� �������� � ���� ��� ���������.
    // Proof: 
    //    https://developers.google.com/web/fundamentals/performance/optimizing-content-efficiency/http-caching?hl=ru
    //    https://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html
    //    https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9
    public static class NancyExpirationExtentions
    {
        static readonly CultureInfo EnUs = new CultureInfo("en-US");
        private static readonly string ContextKey = "Universe Expiration State";

        public static void Expires(this NancyModule module, TimeSpan timeSpan = default(TimeSpan), CachingScope scope = CachingScope.Public)
        {
            var expirationState = new ExpirationState { Scope = scope, Time = timeSpan };
            module.Context.Items[ContextKey] = expirationState;
        }

        public static void BindExpiration(this IPipelines applicationPipelines)
        {
            applicationPipelines.AfterRequest.AddItemToStartOfPipeline(delegate(NancyContext context)
            {
                object raw = null;
                if (!context.Items.TryGetValue(ContextKey, out raw))
                    return;

                if (raw != null && !(raw is ExpirationState))
                    throw new InvalidOperationException("'" + ContextKey + "' instance type of Context.Item should be NancyExpirationExtentions.ExpirationState");

                var state = (ExpirationState) raw;
                if (state == null) return;
                // NiceTrace.Message("Apply Expiration on {0}, {1}", state.Time, state.Scope);
                TimeSpan timeSpan = state.Time;
                CachingScope scope = state.Scope;
                DateTime date = DateTime.UtcNow.Add(timeSpan);
                if (scope == CachingScope.None || timeSpan == TimeSpan.Zero || timeSpan == default(TimeSpan))
                {
                    date = new DateTime(2000, 1, 1);
                    context.Response.Headers["Pragma"] = "no-cache";
                }

                context.Response.Headers["Expires"] = date.ToString("R");
                string cacheControl = "no-store";
                long second = Math.Max(0, (long) timeSpan.TotalSeconds);
                if (scope == CachingScope.Public)
                    cacheControl = "max-age=" + second.ToString("0", EnUs);
                else if (scope == CachingScope.Private)
                    cacheControl = "private, max-age=" + second.ToString("0");

                context.Response.Headers["Cache-Control"] = cacheControl;
            });

        }

        private class ExpirationState
        {
            public TimeSpan Time;
            public CachingScope Scope;
        }
    }

    public enum CachingScope
    {
        None,
        Public,
        Private,
    }

}