using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Coinbase.Pro;
using Coinbase.Pro.Models;

//Things to fix:
// Check if already exist open orders
//

namespace Examples
{
   class Program
   {
      static async Task Main(string[] args)
      {

         #region Authentication
         var clientSandbox = new CoinbaseProClient(new Config
         {
            ApiKey = "d9008cc93bae52d705347879543f7a2c",
            Secret = "Fq1KhUlQfsuHBoHrKwmFvh7XELUJpIwUGoSEaz3CliBBwIl0800229gBOHnGP2luTTLNe3ySyEIRtCn+DlVHLQ==",
            Passphrase = "9ws6jpis4fg",
            ApiUrl = "https://api-public.sandbox.pro.coinbase.com"
         });

         var clientPro = new CoinbaseProClient(new Config
         {
            ApiKey = "fd11e4904d42ceac51d0ae06493ee522",
            Secret = "c7cjJY6Nvyr+7JLyhfuMQ59wgzCHs54oMLlwlRKJF/62Nq15xOXytJRmDgXsEBsWcUU/qFOfLCAz/7fQerbx/w==",
            Passphrase = "2793xxbjc7p",
         });
         #endregion

         Console.WriteLine("Lets Rool!");

         // Inicial vars Set a variable to the Documents path.
         string path = @"D:\\GetPrice.csv";
         //--------------------    bet logic
         string ordertype = "";
         decimal size = 0;
         decimal limitPrice = 0m;
         decimal percentageBuySell = 0.025m;//25% - margin de lucro sem fees 5%
         decimal percentageBet = 0.5m; //percentagem de aposta do valor disponivel 50% na compra
         decimal buyLimiteMaxMin = 500;
         List<decimal> accountAvailable = new List<decimal>();
         List<string> accountCoin = new List<string>();
         string orderstatus = "";
         bool fillState = false;
         decimal marketPrice = 0;
         bool checkBuyBTC = true;
         bool checkSellBTC = true;
         bool checkBuyETH = false;
         bool checkSellETH = false; //tem que estar false porque sandbox nao tem ETH e tenho que mudar no fim do codigo tb.

            var client = clientSandbox; //Sandbox
                                       //var client = clientPro; //Live

            //for (int i = 0; i < 1000; i++)
            while(true)
            {
               // Get all products available
               //var coins = await client.MarketData.GetProductsAsync();
               //foreach (var coin in coins)
               //{
               //   //if (coin.Id == "BTC-EUR" || coin.Id == "BTC-USD")
               //   //{
               //      Console.WriteLine($"Coin Id: {coin.Id}");
               //   //}
               //}

               //---------      GET ALL ORDERS
               Console.ForegroundColor = ConsoleColor.Blue;
               var ordersBTC = await client.Orders.GetAllOrdersAsync("open", "BTC-EUR");
                foreach (var order in ordersBTC.Data)
                {
                    Console.WriteLine($"====================================");
                    Console.WriteLine($"Order Coin: {order.ProductId}", Console.ForegroundColor);
                    Console.WriteLine($"Order Price: {order.Price}", Console.ForegroundColor);
                    Console.WriteLine($"Order Status: {order.Status}", Console.ForegroundColor);
                    Console.WriteLine($"Order SpecidiedFunds: {order.SpecifiedFunds}", Console.ForegroundColor);
                    Console.WriteLine($"Order Value: {order.ExecutedValue}", Console.ForegroundColor);
                    Console.WriteLine($"Order Funds: {order.Funds}", Console.ForegroundColor);
                    Console.WriteLine($"Order Fees: {order.FillFees}", Console.ForegroundColor);
                    Console.WriteLine($"Order Side: {order.Side}", Console.ForegroundColor);
                    Console.WriteLine($"Order Type: {order.Type}", Console.ForegroundColor);

                    string ordersStatus = order.Status;
                    string ordersSide = order.Side.ToString();


                    if (ordersSide == "Buy" && ordersStatus == "open") { checkBuyBTC = false; }
                    if (ordersSide == "Sell" && ordersStatus == "open") { checkSellBTC = false; }
                }



                var ordersETH = await client.Orders.GetAllOrdersAsync("open", "ETH-EUR");
                foreach (var order in ordersETH.Data)
                {
                    Console.WriteLine($"====================================");
                    Console.WriteLine($"Order Coin: {order.ProductId}", Console.ForegroundColor);
                    Console.WriteLine($"Order Price: {order.Price}", Console.ForegroundColor);
                    Console.WriteLine($"Order Status: {order.Status}", Console.ForegroundColor);
                    Console.WriteLine($"Order SpecidiedFunds: {order.SpecifiedFunds}", Console.ForegroundColor);
                    Console.WriteLine($"Order Value: {order.ExecutedValue}", Console.ForegroundColor);
                    Console.WriteLine($"Order Funds: {order.Funds}", Console.ForegroundColor);
                    Console.WriteLine($"Order Fees: {order.FillFees}", Console.ForegroundColor);
                    Console.WriteLine($"Order Side: {order.Side}", Console.ForegroundColor);
                    Console.WriteLine($"Order Type: {order.Type}", Console.ForegroundColor);

                    string ordersStatus = order.Status;
                    string ordersSide = order.Side.ToString();


                    if (ordersSide == "Buy" && ordersStatus == "open") { checkBuyETH = false; }
                    if (ordersSide == "Sell" && ordersStatus == "open") { checkSellETH = false; }
                }

                ////---------      GET ALL Fills
                //Console.ForegroundColor = ConsoleColor.Cyan;

                //var fills = await client.Fills.GetFillsByProductIdAsync("BTC-EUR");
                //foreach (var fill in fills.Data)
                //{
                //   Console.WriteLine($"====================================", Console.ForegroundColor);
                //   Console.WriteLine($"fill Price: {fill.ProductId}", Console.ForegroundColor);
                //   Console.WriteLine($"fill Price: {fill.Price}", Console.ForegroundColor);
                //   Console.WriteLine($"fill Liquidity: {fill.Liquidity}", Console.ForegroundColor);
                //   Console.WriteLine($"fill Fee: {fill.Fee}", Console.ForegroundColor);
                //   Console.WriteLine($"fill Settled: {fill.Settled}", Console.ForegroundColor);
                //   Console.WriteLine($"fill Size: {fill.Size}", Console.ForegroundColor);
                //   Console.WriteLine($"fill UsdVolume: {fill.UsdVolume}", Console.ForegroundColor);
                //}


                //---------      GET ALL MARKETS VALUES
                Console.ForegroundColor = ConsoleColor.White;

               var market = await client.MarketData.GetTickerAsync("BTC-EUR");
               Console.WriteLine($"====================================", Console.ForegroundColor);
               Console.WriteLine($"Market Ask: {market.Ask}", Console.ForegroundColor);
               Console.WriteLine($"Market Bid: {market.Bid}", Console.ForegroundColor);
               Console.WriteLine($"Market Price: {market.Price}", Console.ForegroundColor);
               Console.WriteLine($"Market Size: {market.Size}", Console.ForegroundColor);
               Console.WriteLine($"Market Time: {market.Time}", Console.ForegroundColor);
               Console.WriteLine($"Market TradeId: {market.TradeId}", Console.ForegroundColor);
               Console.WriteLine($"Market Volume: {market.Volume}", Console.ForegroundColor);
               Console.WriteLine($"====================================");

               marketPrice = market.Price;

               using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
               {
                  file.WriteLine("BTC-EUR" + "," + market.Price + "," + market.Ask + "," + market.Bid + "," + market.Time);
               }

               //get accounts (portfolio)
               Console.ForegroundColor = ConsoleColor.Yellow;

               var accounts = await client.Accounts.GetAllAccountsAsync();
               foreach (var account in accounts)
               {
                  if (account.Currency == "EUR" || account.Currency == "BTC" || account.Currency == "ETH")
                  {
                     Console.WriteLine($"====================================", Console.ForegroundColor);
                     Console.WriteLine($"fill Price: {account.Currency}", Console.ForegroundColor);
                     Console.WriteLine($"fill Liquidity: {account.Available}", Console.ForegroundColor);

                     accountCoin.Add(account.Currency);
                     accountAvailable.Add(account.Available);
                  }
               }

               for (int e = 0; e < accountAvailable.Count; e++)
               {
                    //-------------------------   BUY
                    //check if there`s open orders
                    if (accountAvailable[e] > 0 && accountCoin[e] == "EUR" && checkBuyBTC == true || checkBuyETH == true) //All orders goes in btc
                  {

                     //BUY logic
                     ordertype = "Buy";
                     if (accountAvailable[e] <= buyLimiteMaxMin) { percentageBet = 1; }
                     limitPrice = market.Price - (market.Price * percentageBuySell);
                     size = (accountAvailable[e] * percentageBet) / limitPrice;
                     decimal sizeround = Decimal.Round(size, 7);
                     decimal limitPriceRound = Decimal.Round(limitPrice, 2);

                     //  place order limite & error handdling 
                     try
                     {
                        var order1 = await client.Orders.PlaceLimitOrderAsync(
                        OrderSide.Buy, "BTC-EUR", size: sizeround, limitPrice: limitPriceRound, timeInForce: TimeInForce.GoodTillCanceled);
                     }
                     catch (Exception ex)
                     {
                        Console.ForegroundColor = ConsoleColor.Red;
                        var errorMsg = await ex.GetErrorMessageAsync();
                        Console.WriteLine(errorMsg, Console.ForegroundColor);
                     }

                     Console.ForegroundColor = ConsoleColor.Green;
                  Console.WriteLine("BTC-EUR" + "," + market.Price + "," + market.Ask + "," + market.Bid + "," + market.Time + "," + ordertype + "," + size + "," + limitPrice, Console.ForegroundColor);

                     using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                     {
                        file.WriteLine("BTC-EUR" + "," + market.Price + "," + market.Ask + "," + market.Bid + "," + market.Time + "," + ordertype + "," + size + "," + limitPrice);
                     }

                  }

                  //-------------------------   Sell
                  if (accountAvailable[e] > 0 && accountCoin[e] == "BTC" || accountAvailable[e] > 0 && accountCoin[e] == "ETH" && checkSellBTC == true || checkSellETH == true)
                  {
                     //SELL logic
                     ordertype = "Sell";
                     limitPrice = market.Price + (market.Price * percentageBuySell);
                     size = accountAvailable[e];
                     decimal sizeround = Decimal.Round(size, 8);
                     decimal limitPriceRound = Decimal.Round(limitPrice, 2);

                     //  place order limite & error handdling 
                     try
                     {
                        var order1 = await client.Orders.PlaceLimitOrderAsync(
                        OrderSide.Sell, accountCoin[e] + "-EUR", size: sizeround, limitPrice: limitPriceRound, timeInForce: TimeInForce.GoodTillCanceled);
                     }
                     catch (Exception ex)
                     {
                        Console.ForegroundColor = ConsoleColor.Red;
                        var errorMsg = await ex.GetErrorMessageAsync();
                        Console.WriteLine(errorMsg, Console.ForegroundColor);
                     }


                  Console.ForegroundColor = ConsoleColor.Green;
                  Console.WriteLine("BTC-EUR" + "," + market.Price + "," + market.Ask + "," + market.Bid + "," + market.Time + "," + ordertype + "," + size + "," + limitPrice, Console.ForegroundColor);

                  using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                     {
                        file.WriteLine(accountCoin[e] + "-EUR" + "," + market.Price + "," + market.Ask + "," + market.Bid + "," + market.Time + "," + ordertype + "," + size + "," + limitPrice);
                     }
                    }
               }

               /////  --------------    paging
               /////
               ////Get the initial page, items 16 through 20
               //var trades = await client.MarketData.GetTradesAsync("BTC-EUR", limit: 5);
               ////Some time advances, trades execute.
               ////Now, get the next batch of newer trades before the current page.
               //while (trades.Before.HasValue)
               //{
               //   trades = await client.MarketData.GetTradesAsync("BTC-EUR", limit: 5, before: trades.Before);
               //}

               Thread.Sleep(10000);

                checkBuyBTC = true;
                checkSellBTC = true;
                checkBuyETH = false;
                checkSellETH = false;


            }
      

         Console.ReadKey();


      }
   }


}
