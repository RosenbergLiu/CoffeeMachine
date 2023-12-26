# CoffeeMachine API
This repository has two branches

## "original" branch
The original requirements of the test. No extra-credit part

## "extra-credit" branch
The original requirements plus the extra-credit requirements. This branch has multiple operation modes:

### Release Mode:
In standard operational mode, the WebAPI captures the client's IP address to determine the geographic location and obtain the relevant weather data for that location. The weather information is then fetched from OpenWeather API and display relative message based on the weather temperature.

### Debug Mode:
While in debug mode, the client's IP address is hardcoded to a public IP based in Melbourne. Consequently, the weather data retrieved will consistently pertain to Melbourne, regardless of the actual location of the user. This does not effect the testing as tesing uses mock temperature.

### Special Considerations:
##### Localhost Requests:
When the WebAPI is accessed via localhost (i.e., the server is running on the same machine as the client), the weather function will be unavailable. This is due to the loopback IP address used by localhost, which cannot be utilized to look up geographic locations. The displayed message will always be “Your piping hot coffee is ready” as default.

##### IP Lookup Limitation:
It's important to note that the accuracy of the location and, therefore, the weather data depends on the reliability of the ip-api.com lookup service. Discrepancies might occur, especially for IP addresses associated with VPNs or proxies.