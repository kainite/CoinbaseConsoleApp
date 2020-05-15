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
using ConsoleApp_Coinbase;

namespace Examples
{
    class Program
    {
        string mailMessage = "";
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

            #region INICIAL VARIABLES
            // Inicial vars Set a variable to the Documents path.
            string version = ConfigurationManager.AppSettings["version"];  //@"D:\\GetPrice.csv";
            string pathgetprice = ConfigurationManager.AppSettings["pathgetprice"];  //@"D:\\GetPrice.csv";
            string pathbuysell = ConfigurationManager.AppSettings["pathbuysell"];  //@"D:\\GetPrice.csv";
            string ordertype = "";
            decimal size = 0;
            decimal limitPrice = 0m;
            decimal percentageBuySell = Convert.ToDecimal(ConfigurationManager.AppSettings["percentageBuySell"]);  //0.025m;//25% - margin de lucro sem fees 5%
            decimal percentageBet = Convert.ToDecimal(ConfigurationManager.AppSettings["percentageBet"]);  //0.5m; //percentagem de aposta do valor disponivel 50% na compra
            decimal buyLimiteMaxMin = Convert.ToDecimal(ConfigurationManager.AppSettings["buyLimiteMaxMin"]);  //500;
            decimal minimumAccountAvailable = Convert.ToDecimal(ConfigurationManager.AppSettings["minimumAccountAvailable"]);  //50;
            decimal percentageBetOffset = Convert.ToDecimal(ConfigurationManager.AppSettings["percentageBetOffset"]);
            List<decimal> accountAvailable = new List<decimal>();
            List<string> accountCoin = new List<string>();
            decimal marketPrice = 0;
            bool checkBuyBTC = true;
            bool checkSellBTC = true;
            bool checkBuyETH = false;
            bool checkSellETH = false; //tem que estar false porque sandbox nao tem ETH e tenho que mudar no fim do codigo tb.
            bool errorbuysell = false;
            bool emailOffSet = false;
            var client = clientSandbox;

            if (ConfigurationManager.AppSettings["Enviroment"] == "Sandbox")
            { client = clientSandbox; }
            else
            { client = clientPro; }//Sandbox var client = clientPro; //Live
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
                Console.ForegroundColor = ConsoleColor.Blue;
                //BTC
                var ordersBTC = await client.Orders.GetAllOrdersAsync("open", "BTC-EUR");
                GetOrders(percentageBetOffset, marketPrice, ref checkBuyBTC, ref checkSellBTC, ordersBTC, ref emailOffSet);
                //ETH
                // var ordersETH = await client.Orders.GetAllOrdersAsync("open", "ETH-EUR");
                //GetOrders(percentageBetOffset, marketPrice, ref checkBuyBTC, ref checkSellBTC, ordersETH, ref emailOffSet);

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
                        Console.WriteLine($"fill Price: {account.Currency}", Console.ForegroundColor);
                        Console.WriteLine($"fill Liquidity: {account.Available}", Console.ForegroundColor);

                        accountCoin.Add(account.Currency);
                        accountAvailable.Add(account.Available);
                    }
                }

                for (int e = 0; e < accountAvailable.Count; e++)
                {
                    errorbuysell = false;
                    //-------------------------   BUY
                    //check if there`s open orders
                    if (accountAvailable[e] > minimumAccountAvailable && accountCoin[e] == "EUR" && checkBuyBTC == true || checkBuyETH == true) //All orders goes in btc
                    {

                        //BUY logic
                        ordertype = "Buy";
                        if (accountAvailable[e] <= buyLimiteMaxMin) { percentageBet = 1; }
                        limitPrice = marketPrice - (marketPrice * percentageBuySell);
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
                            Console.WriteLine("BUY ERROR - " + ex.Message, Console.ForegroundColor);
                            errorbuysell = true;
                        }

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathbuysell, true))
                        {
                            if (errorbuysell == false)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                string message = "BTC-EUR" + "," + marketPrice + "," + ordertype + "," + size + "," + limitPrice.ToString();
                                Console.WriteLine(message, Console.ForegroundColor);
                                file.WriteLine(message);
                                Email.SendEmail(message);

                                //Console.ForegroundColor = ConsoleColor.Green;
                                //Console.WriteLine("BTC-EUR" + "," + market.Price + "," + market.Ask + "," + market.Bid + "," + market.Time + "," + ordertype + "," + size + "," + limitPrice, Console.ForegroundColor);
                                //file.WriteLine("BTC-EUR" + "," + market.Price + "," + market.Ask + "," + market.Bid + "," + market.Time + "," + ordertype + "," + size + "," + limitPrice);

                            }

                        }

                    }

                    //-------------------------   Sell
                    if (accountAvailable[e] > 0 && accountCoin[e] == "BTC" || accountAvailable[e] > 0 && accountCoin[e] == "ETH" && checkSellBTC == true || checkSellETH == true)
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
                            var order1 = await client.Orders.PlaceLimitOrderAsync(
                            OrderSide.Sell, accountCoin[e] + "-EUR", size: sizeround, limitPrice: limitPriceRound, timeInForce: TimeInForce.GoodTillCanceled);
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
                                string message = "BTC-EUR" + "," + marketPrice + "," + ordertype + "," + size + "," + limitPrice.ToString();
                                Console.WriteLine(message, Console.ForegroundColor);
                                file.WriteLine(message);
                                Email.SendEmail(message);
                            }

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

                accountCoin.Clear();
                accountAvailable.Clear();
                checkBuyBTC = true;
                checkSellBTC = true;
                checkBuyETH = false;
                checkSellETH = false;



                Thread.Sleep(10000);

            }

            Console.ReadKey();

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

        private static void GetOrders(decimal percentageBetOffset, decimal marketPrice, ref bool checkBuyBTC, ref bool checkSellBTC, PagedResponse<Order> ordersBTC, ref bool emailOffset)
        {
            if (ordersBTC.Data.Count <= 0) { emailOffset = false; }
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

                //SEND MONITOR ALERT of offset values bigger than percentagebyofset
                decimal percentageOffset = marketPrice / order.Price - 1;
                decimal percentageOffsetRound = Decimal.Round(percentageOffset, 2);
                Console.ForegroundColor = ConsoleColor.Magenta;
                if (percentageOffset <= percentageBetOffset && emailOffset == false)
                {
                    Console.WriteLine($"Price Offset alert higher than existing order in : " + percentageOffsetRound + "%", Console.ForegroundColor);
                    Email.SendEmail("Price Offset alert higher than existing order in : " + percentageOffsetRound + "%");
                    emailOffset = true;
                }
                if (percentageOffset >= percentageBetOffset && emailOffset == false)
                {
                    Console.WriteLine($"Price Offset alert higher than existing order in : " + percentageOffsetRound + "%", Console.ForegroundColor);
                    Email.SendEmail("Price Offset alert higher than existing order in : " + percentageOffsetRound + "%");
                    emailOffset = true;
                }
                Console.ForegroundColor = ConsoleColor.Blue;

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
