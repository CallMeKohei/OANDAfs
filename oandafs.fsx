// ===========================================================================
//  FILE    : oandafs.fsx
//  AUTHOR  : callmekohei <callmekohei at gmail.com>
//  License : MIT license
// ===========================================================================

namespace oandafs

open System
open System.Web

module Oandafs =

    type Environment = Live | Practice | Sandbox

    type Info = { Web:string; ID:string; Token:string }

    type API () =
        member this.Init (env:Environment , ?id:string, ?token:string) : Info =

            {
                Web   = match env with
                        | Sandbox    -> "http://api-sandbox.oanda.com"
                        | Practice   -> "https://api-fxpractice.oanda.com"
                        | Live       -> "https://api-fxtrade.oanda.com"

                ID    = match id with
                        | Some id    -> id
                        | None       -> ""

                Token = match token with
                        | Some token -> token
                        | None       -> ""
            }

    type Uri (url:string) =

        let uriBuilder = new UriBuilder(url)
        let query = HttpUtility.ParseQueryString(uriBuilder.Query)

        member x.ToString = string uriBuilder.Uri

        member x.Query =
            match uriBuilder.Query with
            | "" -> ""
            | _ -> string (uriBuilder.Query.Substring(1))

        member x.AddQuery (n,v) =
            query.Add (n,v)
            uriBuilder.Query <- query.ToString()

        member x.Url = url


    type Requests (info:Info) =

        member x.requests ( uri:Uri, httpMethod:string ) =

            let header =
                ["Content-Type:application/x-www-form-urlencoded"
                ;"X-Accept-Datetime-Format: RFC3339"]

            use c = new System.Net.WebClient()

            match info.Token  with
            | "" ->
                header |> List.iter c.Headers.Add
            | _ ->
                header @ ["Authorization: Bearer " + info.Token ]
                |> List.iter c.Headers.Add

            match httpMethod with
            | "GET" -> c.DownloadString(uri.ToString)
            | "POST" | "DELETE" | "PATCH" -> c.UploadString (uri.Url, httpMethod, uri.Query)
            | _ -> failwith "error"

    type Rates (info:Info) =

         member this.Get_Instruments param =
            let endpoint = info.Web + "/v1/instruments"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

         member this.Get_Prices param =
            let endpoint = info.Web + "/v1/prices"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

         member this.Get_History param =
            let endpoint = info.Web + "/v1/candles"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")


    type Accounts (info:Info) =

        // username is only required on the sandbox.
        member x.Get_accounts (?username:string)  =
            match username with
            | Some username -> username
            | None -> ""
            |> ignore

            let endpoint = info.Web + "/v1/accounts"
            let uri = new Uri(endpoint)
            if username.IsSome then
                [("username",username.Value)] |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.Get_account (account_id) =
            let endpoint = info.Web + "/v1/accounts/" + account_id
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "GET")

        // This method is only effective on the sandbox.
        member x.Create_test_account () =
            let endpoint = info.Web + "/v1/accounts"
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "POST")


    type Orders (info:Info) =

        member x.Create_order param =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/orders"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "POST")

        member x.Get_orders () =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/orders"
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "GET")

        member x.Get_order order_id =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/orders/" + order_id
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "GET")

        member x.Modify_order order_id param =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/orders/" + order_id
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "PATCH")

        member x.Close_order order_id  =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/orders/" + order_id
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "DELETE")


    type Trades (info:Info) =

        member x.Get_trades () =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/trades"
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "GET")

        member x.Get_trade trade_id =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/trades/" + trade_id
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "GET")

        member x.Modify_trade trade_id param =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/trades/" + trade_id
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "PATCH")

        member x.Close_trade trade_id =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/trades/" + trade_id
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "DELETE")


    type Positions (info:Info) =

        member x.Get_positions () =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/positions"
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "GET")

        member x.Get_position (instrument) =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/positions/" + instrument
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "GET")

        member x.Close_position (instrument) =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/positions/" + instrument
            let uri = new Uri(endpoint)
            Requests(info).requests (uri, "DELETE")


    type Transaction (info:Info) =

        member x.Get_transaction_history (?param:list<_>) =

            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/transactions"
            let uri = new Uri(endpoint)

            match param with
            | Some param -> param
            | None -> []
            |> ignore

            if param.IsSome then
                param.Value |> List.iter uri.AddQuery

            Requests(info).requests (uri, "GET")

        member x.Get_transaction (transaction_id) =
            let endpoint = info.Web + "/v1/accounts/" + info.ID + "/transactions/" + transaction_id
            Requests(info).requests ((new Uri(endpoint)), "GET")


    // Forex Labs is only used by 'practic api' or 'live api'.
    type ForexLabs (info:Info) =

        member x.Get_eco_calendar param =
            let endpoint = info.Web + "/labs/v1/calendar"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.Get_historical_position_ratios param =
            let endpoint = info.Web + "/labs/v1/historical_position_ratios"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.Get_historical_spreads param =
            let endpoint = info.Web + "/labs/v1/spreads"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.Get_commitments_of_traders param =
            let endpoint = info.Web + "/labs/v1/commitments_of_traders"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.Get_orderbook param =
            let endpoint = info.Web + "/labs/v1/orderbook_data"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")


    type Period  = {
        hour   : string * string 
        hour12 : string * string 
        day    : string * string 
        week   : string * string 
        month  : string * string 
        month3 : string * string 
        month6 : string * string 
        year   : string * string
    }

    let period : Period = {
        hour   = ( "period","3600"     )
        hour12 = ( "period","43200"    )
        day    = ( "period","86400"    )
        week   = ( "period","604800"   )
        month  = ( "period","2592000"  )
        month3 = ( "period","7776000"  )
        month6 = ( "period","15552000" )
        year   = ( "period","31536000" )
    }

    type Currency  = {
        GPB_JPY : string * string 
        GPB_USD : string * string 
        USD_JPY : string * string 
    }

    let currency : Currency = {
        GPB_JPY = ("instrument","GBP_JPY") 
        GPB_USD = ("instrument","GBP_USD") 
        USD_JPY = ("instrument","USD_JPY") 
    }

    type Currencies  = {
        GBP_JPY : string * string 
        GBP_USD : string * string 
        USD_JPY : string * string 
    }

    let currencies : Currencies = {
        GBP_JPY = ("instruments","GBP_JPY") 
        GBP_USD = ("instruments","GBP_USD") 
        USD_JPY = ("instruments","USD_JPY") 
    }
