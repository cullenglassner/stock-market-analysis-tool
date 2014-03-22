module AnalysisTools

open System
open System.IO
open System.Diagnostics
open System.Collections.Generic
open FSharp.Data
open FSharp.Data.CsvExtensions
open FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting









// To load multiple stocks add a ticker to the list, ie [ "AAPL"; "MSFT"; "GOOG"; ... ]
//let mutable symbolList = new List<string>()
let symbolList = ["MSFT"]









(******                    HELPER FUNCTIONS & GLOBAL VARIABLES :
 ******                    Some helper functions and global variables for main library functions.
 ******)
// URL of a service that generates stock data
[<Literal>]
let url = "http://ichart.finance.yahoo.com/table.csv?s="
// Used for charting, just a global date.
let recentEndDate = DateTime.Now.AddMonths(-5)


// Path to downloaded stock data
let path (stock:String) =  __SOURCE_DIRECTORY__ + "/../data/" + stock + ".csv"
// Only returning 240 days of stock data due to memory limits when looking
// at large data sets. This downloads and saves the data.
let downloadStockPrices stock  =
    let (stockUrl:String) = url + stock
    CsvFile.Load(stockUrl).Truncate(240).Save(path stock)
// Download the stock data
for stock in symbolList do
    downloadStockPrices stock
// Loads the downloaded file.
let getStockPrices stock = CsvFile.Load(path stock)









(******                    STOCK REVRIEVAL :
 ******                    Gets current stock prices in two different formats.
 ******)
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







(******                    MOVING AVERAGE CALCULATIONS :
 ******                    Calculates the moving average over a range.
 ******)
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







(******                    RSI CALCULATIONS :
 ******                    Calculates the RSI of a stock using a mutable list.
 ******)
// Mutable lists requires for recursion using imparative programming techniques
let mutable gainsList = new List<float>()
let mutable lossList  = new List<float>()

// Needed to give the first average gain when no previous data is available (recusrive case)
let firstAvgGain (symbol:String) (date:DateTime) : float =  
    let data = (getStockPrices symbol).Filter(fun row -> row?Date.AsDateTime() > (date.AddDays(-21.0)) && row?Date.AsDateTime() < date )
    let range = [ for row in data.Rows do
                    yield float (row.GetColumn "Close") ]
    let gains = [ for i in 1 .. (range |> List.length) - 1 do 
                    if range.Item(i) > range.Item(i - 1) then
                        yield range.Item(i) - range.Item(i - 1) ]
    List.average gains
// Gets all other gains to calculate the RS
let rec avgGain (symbol:String) (date:DateTime) (range:float) : float =
    let mutable tmp = 0.0
    if range = 0.0 then
        gainsList.Clear()
        tmp <- firstAvgGain symbol date
    else
        let currGain = firstAvgGain symbol date
        tmp <- ( ( avgGain symbol (date.AddDays(-1.0)) (range - 1.0) ) * 13.0 + currGain ) / 14.0
    gainsList.Add(tmp)
    tmp

// Needed to give the first average loss when no previous data is available (recusrive case)
let firstAvgLoss (symbol:String) (date:DateTime) : float =  
    let data = (getStockPrices symbol).Filter(fun row -> row?Date.AsDateTime() > (date.AddDays(-21.0)) && row?Date.AsDateTime() < date )
    let range = [ for row in data.Rows do
                    yield float (row.GetColumn "Close") ]
    let loss = [ for i in 1 .. (range |> List.length) - 1 do 
                    if range.Item(i) < range.Item(i - 1) then
                        yield range.Item(i) - range.Item(i - 1) ]
    List.average loss
// Gets all other losses to calculate the RS
let rec avgLoss (symbol:String) (date:DateTime) (range:float) : float =
    let mutable tmp = 0.0
    if range = 0.0 then
        lossList.Clear()
        tmp <- firstAvgLoss symbol date
    else
        let currLoss = firstAvgLoss symbol date
        tmp <- ( ( avgLoss symbol (date.AddDays(-1.0)) (range - 1.0) ) * 13.0 + currLoss ) / 14.0
    lossList.Add(tmp)
    tmp


// Relative Strenght of the stock
let RS (symbol:String) (date:DateTime) (range:float) =
    avgGain symbol date range / avgLoss symbol date range

// Relative Strength Index of the stock.
// Magic number 14 is statistically proven to work.
// EQUATION REFERENCE : http://stockcharts.com/help/doku.php?id=chart_school:technical_indicators:relative_strength_in
let RSI (symbol:String) (date:DateTime) : float = 
    100.0 - ( 100.0 / ( 1.0 + RS symbol date 14.0 ))

