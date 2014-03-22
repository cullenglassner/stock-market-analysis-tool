module AnalysisTools

open System
open System.IO
open System.Diagnostics
open System.Collections.Generic
open FSharp.Data
open FSharp.Data.CsvExtensions
open FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting

// URL of a service that generates stock data
[<Literal>]
let url = "http://ichart.finance.yahoo.com/table.csv?s="
let path (stock:String) =  __SOURCE_DIRECTORY__ + "/../data/" + stock + ".csv"

// To load multiple stocks add a ticker to the list, ie [ "AAPL"; "MSFT"; "GOOG"; ... ]
//let mutable symbolList = new List<string>()
let symbolList = ["AAPL"]


// Only returning 240 days of stock data due to memory limits when looking
// at large data sets. This downloads and saves the data.
let downloadStockPrices stock  =
    let (stockUrl:String) = url + stock
    CsvFile.Load(stockUrl).Truncate(240).Save(path stock)

for stock in symbolList do
    downloadStockPrices stock

// Loads the downloaded file.
let getStockPrices stock = CsvFile.Load(path stock)

// Used for charting.
let recentEndDate = DateTime.Now.AddMonths(-5)




(*                    Basic Stock Retrievals                    *)
// Run C# loops in F# : "for ... do ... yield ..."
// Use F# lambda functions : "(fun <params> -> <expr>)"
let recentPrices (symbol:String) = 
    let data = (getStockPrices symbol).Filter(fun row -> row?Date.AsDateTime() > recentEndDate )
    [ for row in data.Rows do
        yield ((row.GetColumn "Date"):String), ((row.GetColumn "Close"):String) ]

let recentPricesStock (symbol:String) = 
    let data = (getStockPrices symbol).Filter(fun row -> row?Date.AsDateTime() > recentEndDate )
    [ for row in data.Rows do
        yield ((row.GetColumn "Date"):String), ((row.GetColumn "High"):String), ((row.GetColumn "Low"):String), ((row.GetColumn "Open"):String), ((row.GetColumn "Close"):String) ]





(*                    Moving Average Calculations                    *)
// @params:
//      symbol      stock symbol
//      date        date for reverse lookup of average
//      endDate     how far back to go to get the average
let getAvg (symbol:String) (date:DateTime) (endDate:DateTime) = 
    let data = (getStockPrices symbol).Filter(fun row -> row?Date.AsDateTime() > endDate && row?Date.AsDateTime() < date )
    let range = [ for row in data.Rows do
                    yield float (row.GetColumn "Close") ]
    List.average range
// @function:       calculate the list of moving averages for a stock over a range
// @params:
//      symbol      stock symbol
//      range       how far back to go to get the average
let movingAvg (symbol:String) (range:float) = 
    let data = (getStockPrices symbol).Filter(fun row -> row?Date.AsDateTime() > recentEndDate )
    [ for row in data.Rows do
        let date = System.DateTime.Parse (row.GetColumn "Date")
        let regressEndDate = date.AddDays((range*(7.0/5.0)) * -1.0)
        yield (date.ToString("yyyy-mm-dd")), (sprintf "%f" (getAvg symbol date regressEndDate)) ]




(*                    RSI Calculations                    *)
let firstAvgGain (symbol:String) (date:DateTime) : float =  
    let data = (getStockPrices symbol).Filter(fun row -> row?Date.AsDateTime() > (date.AddDays(-21.0)) && row?Date.AsDateTime() < date )
    let range = [ for row in data.Rows do
                    yield float (row.GetColumn "Close") ]
    let gains = [ for i in 1 .. (range |> List.length) - 1 do 
                    if range.Item(i) > range.Item(i - 1) then
                        yield range.Item(i) - range.Item(i - 1) ]
    List.average gains
let rec avgGain (symbol:String) (date:DateTime) : float =
    let currGain = firstAvgGain symbol date
    ( avgGain symbol (date.AddDays(-1.0)) * 13.0 + currGain ) / 14.0

let firstAvgLoss (symbol:String) (date:DateTime) : float =  
    let data = (getStockPrices symbol).Filter(fun row -> row?Date.AsDateTime() > (date.AddDays(-21.0)) && row?Date.AsDateTime() < date )
    let range = [ for row in data.Rows do
                    yield float (row.GetColumn "Close") ]
    let loss = [ for i in 1 .. (range |> List.length) - 1 do 
                    if range.Item(i) < range.Item(i - 1) then
                        yield range.Item(i) - range.Item(i - 1) ]
    List.average loss
let rec avgLoss (symbol:String) (date:DateTime) : float =
    let currLoss = firstAvgLoss symbol date
    ( avgLoss symbol (date.AddDays(-1.0)) * 13.0 + currLoss ) / 14.0


let RS (symbol:String) (date:DateTime) : float =
    avgGain symbol date / avgLoss symbol date

let RSI (symbol:String) (date:DateTime) : float = 
    100.0 - ( 100.0 / ( 1.0 + RS symbol date ))









//[<EntryPoint>]
//let main argv = 
//    if argv.Length < 1 then
//        printfn "%A\n" argv
//        printfn "usage: [<stock>; <stock; ...]"
//    symbolList.AddRange(argv)
//    printfn "LIST: %A" symbolList
//
//    System.Console.ReadKey() |> ignore
//    0 // return an integer exit code

