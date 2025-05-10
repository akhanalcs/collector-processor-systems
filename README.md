# collector-processor-systems
Implementing separate collector and processor systems using .NET and Azure cloud.

## Helpful links
1. [Dapr PubSub with Service Bus](https://github.com/Azure-Samples/pubsub-dapr-csharp-servicebus?tab=readme-ov-file)
2. [Monorepo in .NET](https://stackoverflow.com/a/79165870/8644294)
3. [Monorepo in .NET + GitHub](https://medium.com/@no1.melman10/monorepo-net-github-ea179a2ef15e)
4. [Developing with Multiple Repositories inside a Single Solution for .NET](https://devblogs.microsoft.com/ise/dotnet-multi-repo)
5. https://github.com/akhanalcs/iac-on-azure/blob/main/ServiceBus.md (Complete this)
6. Look at Nx: https://github.com/akhanalcs/angular-dotnet-realworld (Delete this repo later after extracting the docs)
   - https://github.com/HaasStefan/ng-journal-insurance-portal (Really nice runnable example of an app with Nx + NestJS + Angular)
7. This repo will swallow https://github.com/akhanalcs/react-aspnetcore-bff eventually.

## Local setup
1. Install k6 for reliability testing.
   ```bash
   $brew install k6
   $ k6 --version
   k6 v1.0.0-rc1 (go1.24.1, darwin/arm64)
   ```
   https://grafana.com/docs/k6/latest/set-up/install-k6/

   And add extension to your IDE.
   https://plugins.jetbrains.com/plugin/16141-k6

   - Open terminal
   - Create a script
     ```js
     k6 new
     ```
   - Put the correct url, adjust options if you'd like, and run it
     ```js
     k6 run script.js
     ```
2. Install Dapr CLI.
   https://docs.dapr.io/getting-started

   ```bash
   $ arch -arm64 brew install dapr/tap/dapr-cli
   $ dapr -v
   CLI version: 1.15.0
   Runtime version: n/a
   ```
3. Initialize Dapr in your local environment

## Note on Auth
A lot of the industry moving away from managing access tokens in the browser and instead using HTTP-only cookies for browser auth which is less susceptible to XSS. You do have to be more careful about CSRF, but that’s less of an issue if you only accept JSON in request bodies rather than conventional form posts. Another upside for cookies is you can authenticate during pre-rendering which is not an option using just access tokens.

Cookies don’t work as well when you have non-browser clients or multiple “audiences” (i.e. independent servers/microservices that need to authenticate requests). This is where OIDC shines.

However you don’t need to give up on the benefits of cookies entirely. The ASP.NET Core server hosting the react app can act as the OIDC client, and then after you successfully redirect back from the SSO to your ASP.NET Core app, you can issue an authentication cookie that can be used by the react app to authenticate API calls back to the ASP.NET Core host. Typically this is done with a combination of AddOpenIdConnect and AddCookie as described in https://learn.microsoft.com/aspnet/core/security/authentication/configure-oidc-web-authentication

If you have any audiences other than your ASP.NET Core Host like a microservice, you can store the access token you acquired on the host during SSO in your authentication cookie. This access token then can be used to do an on-behalf-of flow to get an access token to send to your microservice. You can then validate that access token in you microservice with AddJwtBearer as described in https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication

These access tokens are managed completely by the ASP.NET Core app and are never seen by the browser. Then ASP.NET core app takes request from the browser, authenticates the cookie, and then uses an access token representing the user it is acting on-behalf-of to make any requests necessary to the microservice.

If you just want to pass the request through with an access token instead of a cookie you can use a reverse proxy like YARP as demonstrated in the “Backend For Frontend” or BFF example. https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=bff-pattern

That sample uses Blazor rather than React, but the principles behind BFF are the same since in both cases have app logic running in the browser calling an API on something other than the browser application’s host.

This isn’t to say that you couldn’t just use something like MSAL.js to make requests directly to the microservice from the browser using JWT access tokens assuming the microservice is accessible to the public internet. You might have to do this if your react app is hosted on a static CDN rather than by an ASP.NET Core app.

On the flipside, you could also just use cookies without saving or storing any access tokens anywhere on the browser or server if you can colocate your APIs with the ASP.NET Core app that’s serving the React assets. This tends to be the easiest option if you’re just connecting straight to a DB and not concerned with authenticated access to separate microservice.

It doesn’t matter if the cookie is issued after local Identity UI validates the password against the hash in the database or after the user successfully redirects back from SSO configured by AddOpenIdConnect. They both end up using AddCookie for validating all requests after the initial sign on.

One thing I want to make extra clear is that while you can use JWT access tokens after doing SSO/OIDC, it’s not a requirement. You can just throw away the access tokens and use cookies from then out. That’s what most web applications do.

One of the problems is that there’s almost too many options, and a lot of them can be made to work depending on your scenario. People often start out with more complicated auth infrastructure than they need out of some misguided notion they’re future proofing. So long as you’re using ASP.NET Core’s authentication primitives like the authentication and authorization middleware, it should be easy to change from just cookies to BFF later.

Reference: Microsoft guy on Reddit:
https://www.reddit.com/r/dotnet/comments/1k8dlo2/comment/mp6b19o/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
