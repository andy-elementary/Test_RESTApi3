#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using System.Xml.Serialization;
#endregion

// Add a reference to RESTApiClient
using RESTClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading;

public class RuntimeNetLogic2 : BaseNetLogic
{

    // Access the GET method of RESTClient
    RESTClient.RESTApiClient myRESTClient = new RESTClient.RESTApiClient();
        
    // Code to call GET method of RESTClient with myLabel as parameter
    String myResponse = "";
    int myCode = 0;
    
    public override void Start()
    {
         // Set the value of Text Boxes
        Project.Current.GetVariable("Model/myEndpoint").Value = "https://stable.elementary.team/";
        Project.Current.GetVariable("Model/myRobotID").Value = "0184e8b6-3ae7-62d3-8a07-b09adc2f9d59";
        Project.Current.GetVariable("Model/myOptional").Value = "";
        Project.Current.GetVariable("Model/myToken").Value = "6081f5f03b93daf4018f77c538319590f4f30bfa";
    }

    public override void Stop()
    {
    }



    [ExportMethod]
    public void SetLabel(NodeId labelNodeId)
    {
        Project.Current.GetVariable("Model/myResponse").Value = "";

        String myEndpoint = Project.Current.GetVariable("Model/myEndpoint").Value;
        String myRobotID = Project.Current.GetVariable("Model/myRobotID").Value;
        String myOptional = Project.Current.GetVariable("Model/myOptional").Value;
        String myToken = Project.Current.GetVariable("Model/myToken").Value;
        int myTimeout = Project.Current.GetVariable("Model/Image_Timeout").Value;
        int myRetries = Project.Current.GetVariable("Model/Image_Retries").Value;

        String myInspectID = "";
        String myItemID = "";
        String myImage = "";
        int myCode = 0;

        GetInspectionID(myEndpoint, myRobotID, myToken, out myInspectID, out myCode);
        GetItemID(myEndpoint, myInspectID, myToken, out myItemID, out myCode);
        System.Threading.Thread.Sleep(100);
        GetImage(myEndpoint, myItemID, myToken, out myImage, out myCode);
        if (myImage == "")
        {
            int myCount = 0;
            while (myImage == "" && myCount < myRetries)
            {
                System.Threading.Thread.Sleep(myTimeout);
                GetImage(myEndpoint, myItemID, myToken, out myImage, out myCode);
                myCount++;
                System.Diagnostics.Debug.WriteLine("Retrying: " + myCount.ToString());
            }
        }

        Project.Current.GetVariable("Model/myResponse").Value = myImage;

    }
    private void GetInspectionID(string apiUrl, string queryString, string bearerToken, out string response, out int code)
    {           
        // Clear Return Data
        String myInspectionID = "";
        myResponse = "";
        myCode = 0;

        // Call the GET method of RESTClient to get Inspection_ID using Robot_ID
        myRESTClient.Get(apiUrl + "api/v1/inspections/", "robot_id=" + queryString, bearerToken, out myResponse, out myCode);

        // Error message if the response code is not 200
        if (myCode != 200)
        {
            Console.WriteLine("Error: " + myCode.ToString());
        }
        else
        {
            // Convert the response to a JSON object
            dynamic myJSON = JsonConvert.DeserializeObject(myResponse);
            JObject myJObject = JObject.Parse(myResponse);

           
            // Add error handling code if the inspection_routines array is empty
            if (myJObject["results"][0]["inspection_routines"] != null)
            {
                // Get the inspection_ID from the JSON object at results.0.inspection_routines.0.id
                myInspectionID = (string)myJObject["results"][0]["inspection_routines"][0]["inspection_id"];

            } else {
                Console.WriteLine(myJObject["results"][0]["inspection_routines"]);
            }
        }
        (response, code) = (myInspectionID, myCode);
    }

    private void GetItemID(string apiUrl, string queryString, string bearerToken, out string response, out int code)
    {           
        // Clear Return Data
        String myItemID = "";
        myResponse = "";
        myCode = 0;

        myRESTClient.Get(apiUrl + "api/v1/items/", "inspection_id=" + queryString, bearerToken, out myResponse, out myCode);

        // Error message if the response code is not 200
        if (myCode != 200)
        {
            Console.WriteLine("Error: " + myCode.ToString());
        }
        else
        {
            // Convert the response to a JSON object
            dynamic myJSON = JsonConvert.DeserializeObject(myResponse);
            JObject myJObject = JObject.Parse(myResponse);

            // Add error handling code if the inspection_routines array is empty
            if (myJObject["results"][0] != null)
            {
                // Get the item_ID from the JSON object at results.0.id
                myItemID = (string)myJObject["results"][0]["id"];
            } else {
                Console.WriteLine(myJObject["results"][0]["id"]);
            } 
        }
        (response, code) = (myItemID, myCode);
    }
    
private void GetImage(string apiUrl, string queryString, string bearerToken, out string response, out int code)
    {           
        // Clear Return Data
        String myImageURL = "";
        myResponse = "";
        myCode = 0;

        myRESTClient.Get(apiUrl + "api/v1/items/" + queryString + "/", "", bearerToken, out myResponse, out myCode);

        // Error message if the response code is not 200
        if (myCode != 200)
        {
            Console.WriteLine("Error: " + myCode.ToString());
        }
        else
        {
            // Convert the response to a JSON object
            dynamic myJSON = JsonConvert.DeserializeObject(myResponse);
            JObject myJObject = JObject.Parse(myResponse);

            // Add error handling code if the image array is empty
            if (myJObject["pictures"][0]["image"] != null)
            {
                // Get the image URL from the JSON object at pictures.0.image
                myImageURL = (string)myJObject["pictures"][0]["image"];
            } else {
                Console.WriteLine(myJObject["pictures"][0]);
            }          
        }
        (response, code) = (myImageURL, myCode);
    }
}
