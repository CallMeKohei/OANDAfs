namespace Oanda

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Net
(* open FSharp.Data *)
(* open FSharp.Data.JsonExtensions *)

module public Oanda =



    type Environment = Live | Practice | Sandbox
    type Info = { Web:string; ID:string; Token:string }

    type API () =
        member this.Init (env:Environment , ?id:string, ?token:string) : Info =

            {
                Web   = match env with
                        | Sandbox  -> "http://api-sandbox.oanda.com"
                        | Practice -> "https://api-fxpractice.oanda.com"
                        | Live     -> "https://api-fxtrade.oanda.com"

                ID    = match id with
                        | Some id -> id
                        | None    -> ""

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

        (* member x.Close_orders () = *)
        (*     let endpoint = info.Web + "/v1/accounts/" + info.ID + "/orders" *)
        (*     let uri = new Uri(endpoint) *)
        (*     let response = Requests(info).requests (uri, "GET") *)
        (*     let json = JsonValue.Parse response *)
        (*     json?orders.AsArray () *)
        (*     |> Array.map (fun rcd -> rcd?id) *)
        (*     |> Array.map (fun x -> string x) *)
        (*     |> Array.rev *)
        (*     |> Array.map (fun x -> Orders(info).Close_order x) *)


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

        (* member x.Close_trades () = *)
        (*     let endpoint = info.Web + "/v1/accounts/" + info.ID + "/trades" *)
        (*     let uri = new Uri(endpoint) *)
        (*     let response = Request(info).requests (uri, "GET") *)
        (*     let json = JsonValue.Parse response *)
        (*     json?trades.AsArray () *)
        (*     |> Array.map (fun rcd -> rcd?id) *)
        (*     |> Array.map (fun x -> string x) *)
        (*     |> Array.rev *)
        (*     |> Array.map (fun x -> Trades(info).Close_trade x) *)


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

        member x.get_eco_calendar param =
            let endpoint = info.Web + "/labs/v1/calendar"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.get_historical_position_ratios param =
            let endpoint = info.Web + "/labs/v1/historical_position_ratios"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.get_historical_spreads param =
            let endpoint = info.Web + "/labs/v1/spreads"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.get_commitments_of_traders param =
            let endpoint = info.Web + "/labs/v1/commitments_of_traders"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")

        member x.get_orderbook param =
            let endpoint = info.Web + "/labs/v1/orderbook_data"
            let uri = new Uri(endpoint)
            param |> List.iter uri.AddQuery
            Requests(info).requests (uri, "GET")
