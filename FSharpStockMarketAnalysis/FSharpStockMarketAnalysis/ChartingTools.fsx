#r "../packages/FSharp.Data.2.0.4/lib/net40/FSharp.Data.dll"
#load "../packages/FSharp.Charting.0.90.6/FSharp.Charting.fsx"
#load "AnalysisTools.fs"

open System
open FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting




(*                    Charting Functions                    *)
// Draws the dashed grid for asthetics
let dashGrid = 
    ChartTypes.Grid( LineColor = System.Drawing.Color.Gainsboro, 
          LineDashStyle = ChartDashStyle.Dash )

// Visualize many stocks with a moving average
// To use Line charting use Chart.Line(recentPrices stock |> List.rev, Name=stock)
// To use Stock charting use Chart.Stock(recentPricesStock stock |> List.rev, Name=stock)
// To use Candlestick charting use Chart.Candlestick(recentPricesStock stock |> List.rev, Name=stock)
Chart.Rows
    [ for stock in AnalysisTools.symbolList do
        yield Chart.Combine
            [ Chart.Stock(AnalysisTools.recentPricesStock stock |> List.rev, Name=stock)                    
              Chart.Line(AnalysisTools.movingAvg stock 15.0 |> List.rev, Name=stock+"_MA15")
              Chart.Line(AnalysisTools.movingAvg stock 50.0 |> List.rev, Name=stock+"_MA50") ]
              |> Chart.WithYAxis(Title="USD", MajorGrid=dashGrid)
              |> Chart.WithXAxis(Title="Closing Date")
              |> Chart.WithLegend(InsideArea = false) 
    ]

let sad = AnalysisTools.firstAvgGain "MSFT" (DateTime.Now.AddMonths(-3))