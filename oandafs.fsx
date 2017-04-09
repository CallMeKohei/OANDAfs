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

        member x.ToString () = string uriBuilder.Uri.OriginalString

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
            | "GET" -> c.DownloadString(uri.ToString())
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



    /// Util


    let toEET (dt:System.DateTime) : string =

        let toRFC (dt:System.DateTime) : string =
            System.Xml.XmlConvert.ToString( dt, System.Xml.XmlDateTimeSerializationMode.Utc )

        let easternZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome")
        System.TimeZoneInfo.ConvertTime( dt ,easternZone )
        |> toRFC


    type Period  = {
        Hour   : string * string 
        Hour12 : string * string 
        Day    : string * string 
        Week   : string * string 
        Month  : string * string 
        Month3 : string * string 
        Month6 : string * string 
        Year   : string * string
    }

    let period : Period = {
        Hour   = ( "period","3600"     )
        Hour12 = ( "period","43200"    )
        Day    = ( "period","86400"    )
        Week   = ( "period","604800"   )
        Month  = ( "period","2592000"  )
        Month3 = ( "period","7776000"  )
        Month6 = ( "period","15552000" )
        Year   = ( "period","31536000" )
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

    type Granularity = {
        S5  : string * string
        S10 : string * string
        S15 : string * string
        S30 : string * string 
        M1  : string * string
        M2  : string * string
        M3  : string * string
        M5  : string * string
        M10 : string * string
        M15 : string * string
        M30 : string * string
        H1  : string * string
        H2  : string * string
        H3  : string * string
        H4  : string * string
        H6  : string * string
        H8  : string * string
        H12 : string * string
        D   : string * string
        W   : string * string
        M   : string * string
    }

    let granularity : Granularity = {
        S5  = ("granularity","S5" )
        S10 = ("granularity","S10")
        S15 = ("granularity","S15")
        S30 = ("granularity","S30")
        M1  = ("granularity","M1" )
        M2  = ("granularity","M2" )
        M3  = ("granularity","M3" )
        M5  = ("granularity","M5" )
        M10 = ("granularity","M10")
        M15 = ("granularity","M15")
        M30 = ("granularity","M30")
        H1  = ("granularity","H1" )
        H2  = ("granularity","H2" )
        H3  = ("granularity","H3" )
        H4  = ("granularity","H4" )
        H6  = ("granularity","H6" )
        H8  = ("granularity","H8" )
        H12 = ("granularity","H12")
        D   = ("granularity","D"  )
        W   = ("granularity","W"  )
        M   = ("granularity","M"  )
    }

    type AlignmentTimezone = {
        Japan : string * string
    }

    let alignmentTimezone : AlignmentTimezone = {
        Japan = ("alignmentTimezone","Japan")
    }

    type DailyAlignment = {
        H0  : string * string
        H1  : string * string
        H2  : string * string
        H3  : string * string
        H4  : string * string
        H5  : string * string
        H6  : string * string
        H7  : string * string
        H8  : string * string
        H9  : string * string
        H10 : string * string
        H11 : string * string
        H12 : string * string
        H13 : string * string
        H14 : string * string
        H15 : string * string
        H16 : string * string
        H17 : string * string
        H18 : string * string
        H19 : string * string
        H20 : string * string
        H21 : string * string
        H22 : string * string
        H23 : string * string
    }

    let dailyAlignment : DailyAlignment = {
        H0  = ("dailyAlignment","0" )
        H1  = ("dailyAlignment","1" )
        H2  = ("dailyAlignment","2" )
        H3  = ("dailyAlignment","3" )
        H4  = ("dailyAlignment","4" )
        H5  = ("dailyAlignment","5" )
        H6  = ("dailyAlignment","6" )
        H7  = ("dailyAlignment","7" )
        H8  = ("dailyAlignment","8" )
        H9  = ("dailyAlignment","9" )
        H10 = ("dailyAlignment","10")
        H11 = ("dailyAlignment","11")
        H12 = ("dailyAlignment","12")
        H13 = ("dailyAlignment","13")
        H14 = ("dailyAlignment","14")
        H15 = ("dailyAlignment","15")
        H16 = ("dailyAlignment","16")
        H17 = ("dailyAlignment","17")
        H18 = ("dailyAlignment","18")
        H19 = ("dailyAlignment","19")
        H20 = ("dailyAlignment","20")
        H21 = ("dailyAlignment","21")
        H22 = ("dailyAlignment","22")
        H23 = ("dailyAlignment","23")
    }

    type IncludeFirst = {
        True  : string * string
        False : string * string
    }

    let includeFirst : IncludeFirst = {
        True  = ("includeFirst","true" )
        False = ("includeFirst","false")
    }
