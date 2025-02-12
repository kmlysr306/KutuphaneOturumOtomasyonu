﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace AspNetWebAPIOAuth.OAuth.Providers

{
    // Refresh token alma işlemleri
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        [HttpGet]
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            Create(context);
        }
        
        public void Create(AuthenticationTokenCreateContext context)
        {
            object owinCollection;
            context.OwinContext.Environment.TryGetValue("Microsoft.Owin.Form#collection", out owinCollection);

            var grantType = ((FormCollection)owinCollection)?.GetValues("grant_type").FirstOrDefault();

            if (grantType == null || grantType.Equals("refresh_token")) return;

            //Dilerseniz access_token'dan farklı olarak refresh_token'ın expire time'ını da belirleyebilir, uzatabilirsiniz 
            context.Ticket.Properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(1);

            context.SetToken(context.SerializeTicket());
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            Receive(context);
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);

            if (context.Ticket == null)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                context.Response.ReasonPhrase = "invalid token";
                return;
            }

            context.SetTicket(context.Ticket);
            context.Ticket.Properties.ExpiresUtc = DateTime.UtcNow.AddYears(1);
        }
    }
}

