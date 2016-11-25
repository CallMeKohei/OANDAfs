#OANDAfs
OANDAfs is a F# wrapper for OANDA's REST API.

##Install
Using git:
```
$ git clone https://github.com/CallMeKohei/OANDAfs.git
```

##Usage

```fsharp
#load @"./Path/To/Oanda.fsx"

module Sample =
    open Oanda

    let env   = Oanda.Environment.Live
    let id    = "1234567"
    let token = "abcdefg...."

    let info = Oanda.API().Init ( env, id, token )

    Oanda.Rates(info).Get_prices [("instruments","EUR_USD")]
    |> printfn "%A"
```
result
```
"{
    "prices" : [
        {
            "instrument" : "EUR_USD",
            "time" : "2016-11-02T02:45:00.311716Z",
            "bid" : 1.10637,
            "ask" : 1.10645
        }
    ]
}"
```


##Examples
see also(en) 'http://developer.oanda.com/rest-live/introduction/'

see also(ja) 'http://developer.oanda.com/docs/jp/'

####Rates
```fsharp
Oanda.Rates(info).Get_instruments [("accountId","2517138");("instruments","AUD_CAD")]
Oanda.Rates(info).Get_prices      [("instruments","EUR_USD")]
Oanda.Rates(info).Get_history     [  ("instrument","EUR_USD")
                                   ; ("count","2")
                                   ; ("candleFormat","midpoint")
                                   ; ("granularity","D")
                                   ; ("dailyAlignment","0")
                                   ; ("alignmentTimezone","America/New_York")]
```
####Acounts
```fsharp
Oanda.Accounts.Get_accounts "oremmy"
Oanda.Accounts.Get_account  "3082510"
Oanda.Accounts.Create_test_account
```

####Orders
```fsharp
Oanda.Orders(info).Create_order [   ("instrument","GBP_JPY")
                                  ; ("units","1000")
                                  ; ("side","buy")
                                  ; ("type","marketIfTouched")
                                  ; ("price","190")
                                  ; ("expiry","2016-04-01T00:00:00Z")]
Oanda.Orders(info).Get_orders   ()
Oanda.Orders(info).Get_order    "10000000017927"
Oanda.Orders(info).Modify_order "10000000017927" [("price","250")]
Oanda.Orders(info).Close_order  "10000000017927"
```

####Trades
```fsharp
Oanda.Trades(info).Get_trades   ()
Oanda.Trades(info).Get_trade    "10000000017568"
Oanda.Trades(info).Modify_trade "10000000017568" [("takeProfit","250")]
Oanda.Trades(info).Close_trade  "10000000017568"
```

###Potisions
```fsharp
Oanda.Positions(info).Get_positions  ()
Oanda.Positions(info).Get_position   "GBP_JPY"
Oanda.Positions(info).Close_position "GBP_JPY"
```

####Transactions History
```fsharp
Oanda.Transaction(info).Get_transaction_history [("type", "ORDER_CANCEL")]
Oanda.Transaction(info).Get_transaction         ("10000000017592"))
```

####Forex Labs
```fsharp
Oanda.ForexLabs(info).get_eco_calendar               [("instrument","EUR_USD");("period","2592000")]
Oanda.ForexLabs(info).get_historical_position_ratios [("instrument","EUR_USD");("period","86400")]
Oanda.ForexLabs(info).get_historical_spreads         [("instrument","EUR_USD");("period","3600")]
Oanda.ForexLabs(info).get_commitments_of_traders     [("instrument","EUR_USD")]
Oanda.ForexLabs(info).get_orderbook                  [("instrument","EUR_USD");("period","3600")]
```

####Streaming
```
not yet
```


##LICENCE
The MIT License (MIT)
