#r "../packages/FSharp.Data.2.0.4/lib/net40/FSharp.Data.dll"
#load "../packages/FSharp.Charting.0.90.6/FSharp.Charting.fsx"
#load "AnalysisTools.fs"

open System
open FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting

// Helper function : Draws the dashed grid for asthetics
let dashGrid = 
    ChartTypes.Grid( LineColor = System.Drawing.Color.Gainsboro, 
          LineDashStyle = ChartDashStyle.Dash )









(******                    INSTRUCTIONS :
 ******
 ******                    To use LINE charting use         Chart.Line(recentPrices stock |> List.rev, Name=stock)
 ******                    To use STOCK charting use        Chart.Stock(recentPricesStock stock |> List.rev, Name=stock)
 ******                    To use CANDLESTICK charting use  Chart.Candlestick(recentPricesStock stock |> List.rev, Name=stock)
 ******                    
 ******                    Run only one chart at a time. As this is a script it will only draw the last chart specified.
 ******)









(******                    EXAMPLE 1 :
 ******                    Draws the stocks trend(s) zoomed in for a better view.
 ******)
Chart.Rows
    [ for stock in AnalysisTools.symbolList do
        yield Chart.Combine
            [ Chart.Stock(AnalysisTools.recentPricesStock stock |> List.rev, Name=stock) ]
              |> Chart.WithYAxis(Title="USD", MajorGrid=dashGrid, Min=(AnalysisTools.recentPricesStock stock |> List.min |> (fun (_, _, l, _, c) -> (float l) - 10.0 )))
              |> Chart.WithXAxis(Title="Closing Date")
              |> Chart.WithLegend(InsideArea = false) 
    ]









(******                    EXAMPLE 2 :
 ******                    Draws a stock trend with Movging Averages for 15 and 50 days
 ******)
Chart.Rows
    [ for stock in AnalysisTools.symbolList do
        yield Chart.Combine
            [ Chart.Stock(AnalysisTools.recentPricesStock stock |> List.rev, Name=stock)                    
              Chart.Line(AnalysisTools.movingAvg stock 15.0 |> List.rev, Name=stock+"_MA15")
              Chart.Line(AnalysisTools.movingAvg stock 50.0 |> List.rev, Name=stock+"_MA50") ]
              |> Chart.WithYAxis(Title="USD", MajorGrid=dashGrid, Min=(AnalysisTools.recentPricesStock stock |> List.min |> (fun (_, _, l, _, c) -> (float l) - 10.0 )))
              |> Chart.WithXAxis(Title="Closing Date")
              |> Chart.WithLegend(InsideArea = false) 
    ]









(******                    EXAMPLE 3 :
 ******                    Draws RSI Lines for FIRST stock in symbolList
 ******                    NOTE: There is a definate bug in calculations however I was not able to fix it.
 ******)
let stock = AnalysisTools.symbolList.Head

let rsis = [ for i in 0..100 do
                yield ((DateTime.Now.AddDays(float i * -1.0)).ToString("yyyy-mm-dd"), (AnalysisTools.RSI stock (DateTime.Now.AddDays(float i * -1.0))).ToString())]
let rsi30 = Seq.initInfinite (fun i -> (i.ToString(),"30"))
let rsi50 = Seq.initInfinite (fun i -> (i.ToString(),"50"))
let rsi70 = Seq.initInfinite (fun i -> (i.ToString(),"70"))
Chart.Combine
     [  Chart.Line( rsis, Name=stock+" RSI")
        Chart.Line( rsi30 |> Seq.take 100, Name="-20 RSI", Color = System.Drawing.Color.Aqua)
        Chart.Line( rsi50 |> Seq.take 100, Name="50 RSI", Color = System.Drawing.Color.Blue)
        Chart.Line( rsi70 |> Seq.take 100, Name="+20 RSI", Color = System.Drawing.Color.Aqua) ]
        |> Chart.WithYAxis(Title="USD", MajorGrid=dashGrid)//, Max=100.0, Min=0.0)
        |> Chart.WithXAxis(Title="Closing Date")
        |> Chart.WithLegend(InsideArea = false)