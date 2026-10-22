# Secure payments recipe (.NET / Stripe) — read before adding payments / checkout / subscriptions

Base your Stripe integration on this. It satisfies Diana's security baseline; the ship-time
gate flags committed Stripe secrets, so deviating can BLOCK the deploy. Adapt to the domain.

## NuGet: Stripe.net

## 1. Secret from env — never committed
    var stripeKey = builder.Configuration["Stripe:SecretKey"]
        ?? throw new InvalidOperationException("Stripe:SecretKey not configured");
    StripeConfiguration.ApiKey = stripeKey;
Real value in env var Stripe__SecretKey. appsettings keeps "Stripe": { "SecretKey": "" }.
The client only ever sees the PUBLISHABLE key (pk_...). Never send sk_/rk_/whsec_ to the client.

## 2. The amount is resolved on the SERVER — never trust the client
The client sends a product id; you look up the price in your own catalog/DB. Never take an
amount/price from the request body.
    // client sends { productId }, NOT { amount }
    var product = await _db.Products.FirstAsync(p => p.Id == req.ProductId);
    var options = new SessionCreateOptions {
        LineItems = new() { new() {
            PriceData = new() {
                Currency = "usd",
                UnitAmount = product.PriceCents,          // server value, in cents
                ProductData = new() { Name = product.Name }
            },
            Quantity = 1
        }},
        Mode = "payment", SuccessUrl = successUrl, CancelUrl = cancelUrl
    };
    var session = await new SessionService().CreateAsync(options,
        new RequestOptions { IdempotencyKey = $"checkout:{userId}:{req.ProductId}:{req.Nonce}" });

## 3. Webhook: VERIFY the signature, use the raw body, be idempotent
    var json = await new StreamReader(Request.Body).ReadToEndAsync();
    var sig = Request.Headers["Stripe-Signature"];
    var whSecret = _config["Stripe:WebhookSecret"] ?? throw new InvalidOperationException("not set");
    Stripe.Event evt;
    try { evt = EventUtility.ConstructEvent(json, sig, whSecret); }  // verifies signature
    catch (StripeException) { return BadRequest(); }                 // reject forged events
    // idempotency: skip if evt.Id was already processed (persist handled ids)
    // fulfill on checkout.session.completed / payment_intent.succeeded
- The webhook route is ANONYMOUS (Stripe calls it) but protected BY the signature check.
- Mark an order paid ONLY from the verified webhook — never from the browser success redirect.