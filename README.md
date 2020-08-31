# CoinbaseConsoleApp
Console App Bot to Buy and Sell any cryptocurrency on Coinbase Pro

Check my video on Youtube:
https://www.youtube.com/watch?v=caBGyU2HufU&t=1098s

Configurations:
   add key="version" value="01.01.01"/>  <!-- Code version -->
   add key="pathgetprice" value="D:\GetPrice.csv"/> <!-- market price log -->
   add key="pathbuysell" value="D:\BuySell.csv"/> <!-- buy & sell log -->

Trade Configurations:    
   add key="placebuy" value="yes"/> <!-- Place Orders to Buy-->
   add key="placesell" value="yes"/> <!-- Place Orders to Sell -->
   add key="tradeBTC" value="yes"/> <!-- Trade in BTC -->
   add key="tradeETH" value="no"/> <!-- Trade in ETH -->
    
Trade Settings:    
   add key="buyLimiteMaxMin" value="500"/> <!-- Max value to buy $/€ -->
   add key="percentageBuySell" value="0.025"/> <!-- Percentage variance to buy and sell ex: Buy a 2.5% below the value and sell 2.5% above the current value -->
   add key="percentageBet" value="0.99"/> <!-- Percentage to sell total money -->
   add key="minimumAccountAvailable" value="50"/> <!-- Minimum available in account to buy in $/€ -->
