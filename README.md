# CoinbaseConsoleApp
Console App Bot to buy and sell any cryptocurrency on Coinbase Pro

Console Settings before compile: 

    <!-- SandBox - Get your Keys from Sanbox https://api-public.sandbox.pro.coinbase.com -->
    <add key="ApiKey" value=""/>
    <add key="Secret" value=""/>
    <add key="Passphrase" value=""/>
    <add key="Enviroment" value="Sandbox"/> 
    
    
    <!-- Production -- Get your Keys from Production
    <add key="ApiKey" value=""/>
    <add key="Secret" value=""/>
    <add key="Passphrase" value=""/>
	  -->
	  
  <add key="placebuy" value="no"/> <!-- Place Orders to Buy -->
  <add key="placesell" value="no"/> <!-- Place Orders to Sell -->
  <add key="tradeBTC" value="no"/> <!-- Trade BTC -->
  <add key="tradeETH" value="no"/> <!-- Trade ETH -->
  <add key="pathgetprice" value="D:\GetPrice.csv"/> <!-- market price log change drive on your pc-->
  <add key="pathbuysell" value="D:\BuySell.csv"/> <!-- buy & sell log change drive on your pc-->
  <add key="buyLimiteMaxMin" value="500"/> <!-- max value to buy -->
  <add key="percentageBuySell" value="0.025"/> <!-- percentage variance to buy and sell -->
  <add key="percentageBet" value="0.99"/> <!--percentage to sell total money-->
  <add key="minimumAccountAvailable" value="50"/> <!-- minimum availabel in account to buy in EUR -->
 
