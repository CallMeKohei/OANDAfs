[![MIT-LICENSE](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://github.com/callmekohei/OANDAfs/blob/master/LICENSE)


# OANDAfs
OANDAfs is a F# wrapper for OANDA's REST API.

### Install
```
$ git clone https://github.com/CallMeKohei/OANDAfs.git
```

### How to use

```fsharp
#load "/path/to/oandafs.fsx"
open callmekohei.Oandafs

let env   = Environment.Live
let id    = "your id"
let token = "your personal access token"
let info  = API().Init( env, id, token )

Rates(info).Get_Prices [ currencies.GBP_JPY ]
|> stdout.WriteLine
```
result
```json
{
	"prices" : [
		{
			"instrument" : "GBP_JPY",
			"time" : "2017-03-27T00:16:13.742783Z",
			"bid" : 138.255,
			"ask" : 138.283
		}
	]
}
```


### Examples
see also(en) 'http://developer.oanda.com/rest-live/introduction/'

see also(ja) 'http://developer.oanda.com/docs/jp/'

### Rates
```fsharp
Rates(info).Get_instruments [("accountId","2517138");("instruments","AUD_CAD")]

Rates(info).Get_prices      [("instruments","EUR_USD")]

Rates(info).Get_history     [    ("instrument","EUR_USD")
                               ; ("count","2")
                               ; ("candleFormat","midpoint")
                               ; ("granularity","D")
                               ; ("dailyAlignment","0")
                               ; ("alignmentTimezone","America/New_York")]
```
### Acounts
```fsharp
Accounts.Get_accounts "oremmy"
Accounts.Get_account  "3082510"
Accounts.Create_test_account
```

### Orders
```fsharp
Orders(info).Create_order [   ("instrument","GBP_JPY")
                                  ; ("units","1000")
                                  ; ("side","buy")
                                  ; ("type","marketIfTouched")
                                  ; ("price","190")
                                  ; ("expiry","2016-04-01T00:00:00Z")]

Orders(info).Get_orders   ()

Orders(info).Get_order    "10000000017927"

Orders(info).Modify_order "10000000017927" [("price","250")]

Orders(info).Close_order  "10000000017927"
```

### Trades
```fsharp
Trades(info).Get_trades   ()

Trades(info).Get_trade    "10000000017568"

Trades(info).Modify_trade "10000000017568" [("takeProfit","250")]

Trades(info).Close_trade  "10000000017568"
```

### Potisions
```fsharp
Positions(info).Get_positions  ()

Positions(info).Get_position   "GBP_JPY"

Positions(info).Close_position "GBP_JPY"
```

### Transactions History
```fsharp
Transaction(info).Get_transaction_history [("type", "ORDER_CANCEL")]

Transaction(info).Get_transaction         ("10000000017592"))
```

### Forex Labs
```fsharp
ForexLabs(info).get_eco_calendar               [("instrument","EUR_USD");("period","2592000")]

ForexLabs(info).get_historical_position_ratios [("instrument","EUR_USD");("period","86400")]

ForexLabs(info).get_historical_spreads         [("instrument","EUR_USD");("period","3600")]

ForexLabs(info).get_commitments_of_traders     [("instrument","EUR_USD")]

ForexLabs(info).get_orderbook                  [("instrument","EUR_USD");("period","3600")]
```

### Streaming
```
not yet
```
