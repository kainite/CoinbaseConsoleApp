using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Coinbase.Pro;
using Coinbase.Pro.Models;


namespace Examples
{
    class Program
    {
        string mailMessage = "";
        static async Task Main(string[] args)
        {
            #region Authentication
            CoinbaseProClient clientSandbox, clientPro;
            GetAuthentication(out clientSandbox, out clientPro);
            #endregion

            #region INICIAL VARIABLES
            // Inicial vars Set a variable to the Documents path.
            string version = ConfigurationManager.AppSettings["version"]; 
            string pathgetprice = ConfigurationManager.AppSettings["pathgetprice"]; 
            string pathbuysell = ConfigurationManager.AppSettings["pathbuysell"];
            string tradeBTC = ConfigurationManager.AppSettings["tradeBTC"];
            string tradeETH = ConfigurationManager.AppSettings["tradeETH"];
            string ordertype = "";
            decimal size = 0;
            decimal limitPrice = 0m;
            decimal percentageBuySell = Convert.ToDecimal(ConfigurationManager.AppSettings["percentageBuySell"]);  
            decimal percentageBet = Convert.ToDecimal(ConfigurationManager.AppSettings["percentageBet"]); 
            decimal buyLimiteMaxMin = Convert.ToDecimal(ConfigurationManager.AppSettings["buyLimiteMaxMin"]);  
            decimal minimumAccountAvailable = Convert.ToDecimal(ConfigurationManager.AppSettings["minimumAccountAvailable"]);  
            decimal percentageBetOffset = Convert.ToDecimal(ConfigurationManager.AppSettings["percentageBetOffset"]);
            List<decimal> accountAvailable = new List<decimal>();
            List<string> accountCoin = new List<string>();
            string placebuy = ConfigurationManager.AppSettings["placebuy"];
            string placesell = ConfigurationManager.AppSettings["placesell"];
            decimal marketPrice = 0;
            bool checkBuyBTC = true;
            bool checkSellBTC = true;
            bool checkBuyETH = true;
            bool checkSellETH = true; //tem que estar false porque sandbox nao tem ETH e tenho que mudar no fim do codigo tb.
            bool errorbuysell = false;
            var client = clientSandbox;

            if (ConfigurationManager.AppSettings["Enviroment"] == "Sandbox")
            { client = clientSandbox; }
            else
            { client = clientPro; }
            #endregion

            Console.WriteLine("Lets Rock!! Code version:" + version);
            //for (int i = 0; i < 1000; i++)
            while (true)
            {
                // Get all products available
                //var coins = await client.MarketData.GetProductsAsync();
                //GetProducts(coins);

                //---------      GET ALL MARKETS VALUES
                Console.ForegroundColor = ConsoleColor.White;
                var market = await client.MarketData.GetTickerAsync("BTC-EUR");
                marketPrice = GetMarkets(market, pathgetprice);

                //---------      GET ALL ORDERS
                Console.ForegroundColor = ConsoleColor.Cyan;
                //BTC
                var ordersBTC = await client.Orders.GetAllOrdersAsync("open", "BTC-EUR");
                GetOrders(percentageBetOffset, marketPrice, ref checkBuyBTC, ref checkSellBTC, ordersBTC);

                //ETH
                var ordersETH = await client.Orders.GetAllOrdersAsync("open", "ETH-EUR");
                GetOrders(percentageBetOffset, marketPrice, ref checkBuyETH, ref checkSellETH, ordersETH);

                ////---------      GET ALL Fills
                Console.ForegroundColor = ConsoleColor.Cyan;
                //var fills = await client.Fills.GetFillsByProductIdAsync("BTC-EUR");
                //GetFills(fills);


                //Get accounts (portfolio)
                Console.ForegroundColor = ConsoleColor.Yellow;
                var accounts = await client.Accounts.GetAllAccountsAsync();
                foreach (var account in accounts)
                {
                    if (account.Currency == "EUR" || account.Currency == "BTC" || account.Currency == "ETH")
                    {
                        Console.WriteLine($"====================================", Console.ForegroundColor);
                        Console.WriteLine($"Value Price: {account.Currency}", Console.ForegroundColor);
                        Console.WriteLine($"Value Liquidity: {account.Available}", Console.ForegroundColor);

                        accountCoin.Add(account.Currency);
                        accountAvailable.Add(account.Available);
                    }
                }

                for (int e = 0; e < accountAvailable.Count; e++)
                {
                    errorbuysell = false;
                    //-------------------------   BUY
                    //check if there`s open orders
                    if (accountAvailable[e] > minimumAccountAvailable && accountCoin[e] == "EUR" && checkBuyBTC == true ||
                        accountAvailable[e] > minimumAccountAvailable && accountCoin[e] == "EUR" && checkBuyETH == true) //All orders goes in btc
                    {
                        //BUY logic ETH
                        ordertype = "Buy";
                        if (accountAvailable[e] <= buyLimiteMaxMin) { percentageBet = 1; }
                        limitPrice = marketPrice - (marketPrice * percentageBuySell);
                        size = (accountAvailable[e] * percentageBet) / limitPrice;
                        decimal sizeround = Decimal.Round(size, 7);
                        decimal limitPriceRound = Decimal.Round(limitPrice, 2);

                        //  place order limite & error handdling 
                        try
                        {
                            if (placebuy == "yes" && tradeBTC == "yes" || placebuy == "yes" && tradeETH == "yes")
                            { 
                                var order1 = await client.Orders.PlaceLimitOrderAsync(
                                OrderSide.Buy, "ETH-EUR", size: sizeround, limitPrice: limitPriceRound, timeInForce: TimeInForce.GoodTillCanceled);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("BUY ERROR - " + ex.Message, Console.ForegroundColor);
                            errorbuysell = true;
                        }

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathbuysell, true))
                        {
                            if (errorbuysell == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                string message = "ETH-EUR" + "," + marketPrice + "," + ordertype + "," + size + "," + limitPrice.ToString();
                                Console.WriteLine(message, Console.ForegroundColor);
                                file.WriteLine(message);
                                

                            }
                        }
                    }

                    //-------------------------   Sell
                    if (accountAvailable[e] > 0 && accountCoin[e] == "BTC" && checkSellBTC == true || 
                        accountAvailable[e] > 0 && accountCoin[e] == "ETH" && checkSellETH == true)
                    {
                        //SELL logic
                        ordertype = "Sell";
                        limitPrice = marketPrice + (marketPrice * percentageBuySell);
                        size = accountAvailable[e];
                        decimal sizeround = Decimal.Round(size, 8);
                        decimal limitPriceRound = Decimal.Round(limitPrice, 2);

                        //  place order limite & error handdling 
                        try
                        {
                            if (placesell == "yes" && tradeBTC == "yes" || placesell == "yes" && tradeETH == "yes")
                            {
                                var order1 = await client.Orders.PlaceLimitOrderAsync(
                                OrderSide.Sell, accountCoin[e] + "-EUR", size: sizeround, limitPrice: limitPriceRound, timeInForce: TimeInForce.GoodTillCanceled);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("SELL ERROR - " + ex.Message, Console.ForegroundColor);
                            errorbuysell = true;
                        }

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathbuysell, true))
                        {
                            if (errorbuysell == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                string message = accountCoin[e] + "-EUR" + "," + marketPrice + "," + ordertype + "," + size + "," + limitPrice.ToString();
                                Console.WriteLine(message, Console.ForegroundColor);
                                file.WriteLine(message);
                                
                            }

                        }
                    }
                }

                accountCoin.Clear();
                accountAvailable.Clear();
                checkBuyBTC = true;
                checkSellBTC = true;
                checkBuyETH = true;
                checkSellETH = true;

                Thread.Sleep(10000);


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

            }
            //Console.ReadKey();
        }

        private static void GetAuthentication(out CoinbaseProClient clientSandbox, out CoinbaseProClient clientPro)
        {
            clientSandbox = new CoinbaseProClient(new Config
            {
                ApiKey = ConfigurationManager.AppSettings["ApiKey"],
                Secret = ConfigurationManager.AppSettings["Secret"],
                Passphrase = ConfigurationManager.AppSettings["Passphrase"],
                ApiUrl = "https://api-public.sandbox.pro.coinbase.com"
            });
            clientPro = new CoinbaseProClient(new Config
            {
                ApiKey = ConfigurationManager.AppSettings["ApiKey"],
                Secret = ConfigurationManager.AppSettings["Secret"],
                Passphrase = ConfigurationManager.AppSettings["Passphrase"],
            });
        }

        private static void GetProducts(List<Product> coins)
        {
            foreach (var coin in coins)
            {
                //if (coin.Id == "BTC-EUR" || coin.Id == "BTC-USD")
                //{
                Console.WriteLine($"Coin Id: {coin.Id}");
                //}
            }
        }

        private static void GetFills(PagedResponse<Fill> fills)
        {
            foreach (var fill in fills.Data)
            {
                Console.WriteLine($"====================================", Console.ForegroundColor);
                Console.WriteLine($"fill Price: {fill.ProductId}", Console.ForegroundColor);
                Console.WriteLine($"fill Price: {fill.Price}", Console.ForegroundColor);
                Console.WriteLine($"fill Liquidity: {fill.Liquidity}", Console.ForegroundColor);
                Console.WriteLine($"fill Fee: {fill.Fee}", Console.ForegroundColor);
                Console.WriteLine($"fill Settled: {fill.Settled}", Console.ForegroundColor);
                Console.WriteLine($"fill Size: {fill.Size}", Console.ForegroundColor);
                Console.WriteLine($"fill UsdVolume: {fill.UsdVolume}", Console.ForegroundColor);
            }
        }

        private static void GetOrders(decimal percentageBetOffset, decimal marketPrice, ref bool checkBuyBTC, ref bool checkSellBTC, PagedResponse<Order> ordersBTC)
        {
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

                string orderProductId = order.ProductId;
                string ordersStatus = order.Status;
                string ordersSide = order.Side.ToString();
                
                if (ordersSide == "Buy" && ordersStatus == "open") { checkBuyBTC = false; }
                if (ordersSide == "Sell" && ordersStatus == "open") { checkSellBTC = false; }

                //SEND MONITOR ALERT of offset values bigger than percentagebyofset
                decimal percentageOffset = marketPrice / order.Price - 1;
                decimal percentageOffsetRound = Decimal.Round(percentageOffset, 2);
                if (percentageOffsetRound < 0) { percentageOffsetRound = percentageOffsetRound * -1; }

                Console.ForegroundColor = ConsoleColor.Magenta;
                if (percentageOffsetRound <= percentageBetOffset)
                {
                    Console.WriteLine(orderProductId + " Price Offset alert LOWER than existing order in : " + percentageOffsetRound + "%", Console.ForegroundColor);                   
                }
                if (percentageOffsetRound >= percentageBetOffset)
                {
                    Console.WriteLine(orderProductId + " Price Offset alert HIGHER than existing order in : " + percentageOffsetRound + "%", Console.ForegroundColor);
                    
                }
                Console.ForegroundColor = ConsoleColor.Cyan;

            }
        }

        private static decimal GetMarkets(Ticker market, string pathgetprice)
        {
            decimal marketPrice;
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

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathgetprice, true))
            {
                file.WriteLine("BTC-EUR" + "," + market.Price + "," + market.Ask + "," + market.Bid + "," + market.Time);
            }

            return marketPrice;
        }

      }


}
