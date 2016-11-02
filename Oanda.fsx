namespace Oanda
#if INTERACTIVE
#I "./packages/FSharp.Data/lib/net40/"
#r "FSharp.Data.DesignTime.dll"
#r "FSharp.Data.dll"
#endif

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Net
open FSharp.Data
open FSharp.Data.JsonExtensions

module public Oanda =

    type Environment = Sandbox | Practice | Live

    let mutable private  api_url:string      = ""
    let mutable private  account_id :string  = ""
    let mutable private  access_token:string = ""

    type API () =

        static member Init (environment:Environment , ?accountID:string, ?accessToken:string) =

            match environment with
            | Sandbox  ->  api_url <- "http://api-sandbox.oanda.com"
            | Practice ->  api_url <- "https://api-fxpractice.oanda.com"
            | Live     ->  api_url <- "https://api-fxtrade.oanda.com"
            |> ignore

            match accountID with
            | Some accountID -> account_id <- accountID
            | None -> account_id <- ""
            |> ignore

            match accessToken with
            | Some accessToken -> access_token <- accessToken
            | None -> access_token <- ""


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


    type Requests () =

        static member requests ( uri:Uri, httpMethod:string ) =

            let header =
                ["Content-Type:application/x-www-form-urlencoded"
                ;"X-Accept-Datetime-Format: RFC3339"]

            use c = new System.Net.WebClient()

            match access_token with
            | "" ->
                header |> List.iter c.Headers.Add
            | _ ->
                header @ ["Authorization: Bearer " + access_token ]
                |> List.iter c.Headers.Add

            match httpMethod with
            | "GET" -> c.DownloadString(uri.ToString)
            | "POST" | "DELETE" | "PATCH" -> c.UploadString (uri.Url, httpMethod, uri.Query)
            | _ -> failwith "error"


    type Rates () =

        static member Get_instruments param =
            let endpoint = api_url + "/v1/instruments"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")

        static member Get_prices param =
            let endpoint = api_url + "/v1/prices"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")

        static member Get_history param =
            let endpoint = api_url + "/v1/candles"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")


    type Accounts () =

        // username is only required on the sandbox.
        static member  Get_accounts (?username:string)  =
            match username with
            | Some username -> username
            | None -> ""
            |> ignore

            let endpoint = api_url + "/v1/accounts"
            let uri = new Uri(endpoint)
            if username.IsSome then
                [("username",username.Value)] |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")

        static member Get_account (account_id) =
            let endpoint = api_url + "/v1/accounts/" + account_id
            let uri = new Uri(endpoint)
            Requests.requests (uri, "GET")

        // This method is only effective on the sandbox.
        static member Create_test_account () =
            let endpoint = api_url + "/v1/accounts"
            let uri = new Uri(endpoint)
            Requests.requests (uri, "POST")


    type Orders () =

        static member Create_order param =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/orders"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "POST")

        static member Get_orders () =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/orders"
            let uri = new Uri(endpoint)
            Requests.requests (uri, "GET")

        static member Get_order order_id =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/orders/" + order_id
            let uri = new Uri(endpoint)
            Requests.requests (uri, "GET")

        static member Modify_order order_id param =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/orders/" + order_id
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "PATCH")

        static member Close_order order_id  =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/orders/" + order_id
            let uri = new Uri(endpoint)
            Requests.requests (uri, "DELETE")

        static member Close_orders () =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/orders"
            let uri = new Uri(endpoint)
            let response = Requests.requests (uri, "GET")
            let json = JsonValue.Parse response
            json?orders.AsArray ()
            |> Array.map (fun rcd -> rcd?id)
            |> Array.map (fun x -> string x)
            |> Array.rev
            |> Array.map (fun x -> Orders.Close_order x)


    type Trades () =

        static member Get_trades () =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/trades"
            let uri = new Uri(endpoint)
            Requests.requests (uri, "GET")

        static member Get_trade trade_id =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/trades/" + trade_id
            let uri = new Uri(endpoint)
            Requests.requests (uri, "GET")

        static member Modify_trade trade_id param =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/trades/" + trade_id
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "PATCH")

        static member Close_trade trade_id =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/trades/" + trade_id
            let uri = new Uri(endpoint)
            Requests.requests (uri, "DELETE")

        static member Close_trades () =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/trades"
            let uri = new Uri(endpoint)
            let response = Requests.requests (uri, "GET")
            let json = JsonValue.Parse response
            json?trades.AsArray ()
            |> Array.map (fun rcd -> rcd?id)
            |> Array.map (fun x -> string x)
            |> Array.rev
            |> Array.map (fun x -> Trades.Close_trade x)


    type Positions () =

        static member Get_positions () =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/positions"
            let uri = new Uri(endpoint)
            Requests.requests (uri, "GET")

        static member Get_position (instrument) =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/positions/" + instrument
            let uri = new Uri(endpoint)
            Requests.requests (uri, "GET")

        static member Close_position (instrument) =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/positions/" + instrument
            let uri = new Uri(endpoint)
            Requests.requests (uri, "DELETE")


    type Transaction () =

        static member Get_transaction_history (?param:list<_>) =

            let endpoint = api_url + "/v1/accounts/" + account_id + "/transactions"
            let uri = new Uri(endpoint)

            match param with
            | Some param -> param
            | None -> []
            |> ignore

            if param.IsSome then
                param.Value |> List.iter uri.AddQuery

            Requests.requests (uri, "GET")

        static member Get_transaction (transaction_id) =
            let endpoint = api_url + "/v1/accounts/" + account_id + "/transactions/" + transaction_id
            Requests.requests ((new Uri(endpoint)), "GET")


    // Forex Labs is only used by 'practic api' or 'live api'.
    type ForexLabs () =

        static member get_eco_calendar param =
            let endpoint = api_url + "/labs/v1/calendar"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")

        static member get_historical_position_ratios param =
            let endpoint = api_url + "/labs/v1/historical_position_ratios"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")

        static member get_historical_spreads param =
            let endpoint = api_url + "/labs/v1/spreads"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")

        static member get_commitments_of_traders param =
            let endpoint = api_url + "/labs/v1/commitments_of_traders"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")

        static member get_orderbook param =
            let endpoint = api_url + "/labs/v1/orderbook_data"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests.requests (uri, "GET")


