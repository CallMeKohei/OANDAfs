#OANDAfs
OANDAfs is a F# wrapper for OANDA's REST API.

##Install
Using git:
```
$ git clone https://github.com/CallMeKohei/OANDAfs.git
```

create dll:
```
fsharpc
-a
--nologo
--simpleresolution
-r:/path/to/the/FSharp.Data.dll
Oanda.fsx'
```

##Usage
sample code

```fsharp
#r "/path/to/the/Oanda.dll"
#r "/path/to/the/FSharp.Data.dll"

open Oanda
module Sample =

    let accountId = "1234567"
    let accessToken = "abcdefg...."
    Oanda.API.Init ( Oanda.Live ,accountId, accessToken )

    Oanda.Rates.Get_prices [("instruments","EUR_USD")]
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
Oanda.Rates.Get_instruments [("accountId","2517138");("instruments","AUD_CAD")]
Oanda.Rates.Get_prices      [("instruments","EUR_USD")]
Oanda.Rates.Get_history     [  ("instrument","EUR_USD")
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
Oanda.Orders.Create_order [   ("instrument","GBP_JPY")
                            ; ("units","1000")
                            ; ("side","buy")
                            ; ("type","marketIfTouched")
                            ; ("price","190")
                            ; ("expiry","2016-04-01T00:00:00Z")]
Oanda.Orders.Get_orders   ()
Oanda.Orders.Get_order    "10000000017927"
Oanda.Orders.Modify_order "10000000017927" [("price","250")]
Oanda.Orders.Close_order  "10000000017927"
Oanda.Orders.Close_orders ()
```

####Trades
```fsharp
Oanda.Trades.Get_trades   ()
Oanda.Trades.Get_trade    "10000000017568"
Oanda.Trades.Modify_trade "10000000017568" [("takeProfit","250")]
Oanda.Trades.Close_trade  "10000000017568"
Oanda.Trades.Close_trades ()
```

###Potisions
```fsharp
Oanda.Positions.Get_positions  ()
Oanda.Positions.Get_position   "GBP_JPY"
Oanda.Positions.Close_position "GBP_JPY"
```

####Transactions History
```fsharp
Oanda.Transaction.Get_transaction_history [("type", "ORDER_CANCEL")]
Oanda.Transaction.Get_transaction         ("10000000017592"))
```

####Forex Labs
```fsharp
ForexLabs.get_eco_calendar               [("instrument","EUR_USD");("period","2592000")])
ForexLabs.get_historical_position_ratios [("instrument", "EUR_USD");("period","86400")])
ForexLabs.get_historical_spreads         [("instrument","EUR_USD");("period","3600")])
ForexLabs.get_commitments_of_traders     [("instrument","EUR_USD")])
ForexLabs.get_orderbook                  [("instrument","EUR_USD");("period","3600")])
```

####Streaming
```
not yet
```


##LICENCE
The MIT License (MIT)
