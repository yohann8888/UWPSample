﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using EscalationSystem.Models;
using Windows.Web.Http;
using Newtonsoft.Json;
using MyToolkit.Collections;
using Windows.UI.Popups;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using Windows.Storage;
using Windows.Storage.Streams;

namespace EscalationSystem.ViewModels
{
    public class FTEEscalationThreadViewModel: ViewModelBase
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;              
            }
           set
            {
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }

private EscalationStatusWithSelectedItem AllEscalationStatus_List;
        public EscalationStatusWithSelectedItem AllEscalationStatusList
        {
            get
            {
                if(AllEscalationStatus_List==null)
                {
                    AllEscalationStatus_List = new EscalationStatusWithSelectedItem();
                    return AllEscalationStatus_List;
                }

                return AllEscalationStatus_List;
            }
            set
            {
                AllEscalationStatus_List = value;
            }
        }

        private ProductWithSelectedItem AllPratfrom_List;
        public ProductWithSelectedItem AllPratfromList
        {
            get
            {
                if (AllPratfrom_List == null)
                {
                    AllPratfrom_List = new ProductWithSelectedItem();
                    return AllPratfrom_List;
                }

                return AllPratfrom_List;
            }
            set
            {
                AllPratfrom_List = value;
            }
        }

        public FTEEscalationThreadViewModel()
        {
            IsLoading = true;
        }

        public static async Task<FTEEscalationThreadViewModel> GetFTEEscalationThreadViewModel()
        {
            var FTEEscalationThreadViewModel = new FTEEscalationThreadViewModel();
            
            FTEEscalationThreadViewModel.AllEscalationStatusList = new EscalationStatusWithSelectedItem();
            FTEEscalationThreadViewModel.AllEscalationStatusList.MyEscalationStatusList = new ObservableCollection<EscalationStatus>();
            FTEEscalationThreadViewModel.AllEscalationStatusList.MyEscalationStatusList = await FTEEscalationThreadViewModel.GetAllEScalationStatus();
            FTEEscalationThreadViewModel.AllEscalationStatusList.SelectedItem = new EscalationStatus();
            FTEEscalationThreadViewModel.AllEscalationStatusList.SelectedItem = FTEEscalationThreadViewModel.AllEscalationStatusList.MyEscalationStatusList[0];

            FTEEscalationThreadViewModel.AllPratfromList = new ProductWithSelectedItem();
            FTEEscalationThreadViewModel.AllPratfromList.MyProductList = new ObservableCollection<Product>();
            FTEEscalationThreadViewModel.AllPratfromList.MyProductList = await FTEEscalationThreadViewModel.GetAllPlaform();
            FTEEscalationThreadViewModel.AllPratfromList.SelectedItem = new Product();
            FTEEscalationThreadViewModel.AllPratfromList.SelectedItem = FTEEscalationThreadViewModel.AllPratfromList.MyProductList[0];
            return FTEEscalationThreadViewModel;
        }

        public async Task<ObservableCollection<EscalationStatus>> GetAllEScalationStatus()
        {
            HttpClient HttpClient = new HttpClient();
            var HttpResponseMessage = await HttpClient.GetAsync(new Uri("http://escalationmanagerwebapi.azurewebsites.net/api/statuses"));
            ObservableCollection<EscalationStatus> MyEscalationStatus = new ObservableCollection<EscalationStatus>();
            if (HttpResponseMessage.StatusCode == HttpStatusCode.Ok)
            {
                var result = await HttpResponseMessage.Content.ReadAsStringAsync();
                MyEscalationStatus = JsonConvert.DeserializeObject<ObservableCollection<EscalationStatus>>(result);
                MyEscalationStatus.Insert(0,new EscalationStatus() { StatusId = 10, Status = "All", StatusType = "All" });
                MyEscalationStatus.Insert(1, new EscalationStatus() { StatusId = 11, Status = "Open:All", StatusType = "Open:All" });
                MyEscalationStatus.Insert(2, new EscalationStatus() { StatusId = 12, Status = "Closed:All", StatusType = "Closed:All" });
            }
            return MyEscalationStatus;
        }


        public async Task<ObservableCollection<Product>> GetAllPlaform()
        {
            HttpClient HttpClient = new HttpClient();
            var HttpResponseMessage = await HttpClient.GetAsync(new Uri("http://escalationmanagerwebapi.azurewebsites.net/api/products?platform=All"));
            ObservableCollection<Product> AllMyPlatform = new ObservableCollection<Product>();
            if (HttpResponseMessage.StatusCode == HttpStatusCode.Ok)
            {
                var result = await HttpResponseMessage.Content.ReadAsStringAsync();
                AllMyPlatform = JsonConvert.DeserializeObject<ObservableCollection<Product>>(result);
                AllMyPlatform.Insert(0, new Product() { Platform = "All", Forum = "All",Description="All",Owner="All",Operator="All" });

            }

            return AllMyPlatform;
        }

        public async void ModifyStatus(EscalationThread escalationThread)
        {
            HttpClient HttpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(escalationThread);
            var stringContent = new HttpStringContent(json,
                         Windows.Storage.Streams.UnicodeEncoding.Utf8,
                         "application/json");
            var HttpResponseMessage = await HttpClient.PutAsync(new Uri("http://escalationmanagerwebapi.azurewebsites.net/api/ethreads"),stringContent);
            if(HttpResponseMessage.StatusCode==HttpStatusCode.Ok)
            {
                MessageDialog messageDialog = new MessageDialog("Modify the Status Sucessfully!!");
                await messageDialog.ShowAsync();
            }

        }
        public int GetPageIndex(ObservableCollectionView<EscalationAndStatusThread> EscalationThreadList, int pageSize)
        {
            int AllPageIndex;
            if (EscalationThreadList.Count > pageSize)
            {

                if (EscalationThreadList.Count % pageSize == 0)
                {
                    AllPageIndex = EscalationThreadList.Count / pageSize;
                }
                else
                {
                    AllPageIndex = (EscalationThreadList.Count / pageSize) + 1;
                }
            }

            else
            {
                AllPageIndex = 1;
            }

            return AllPageIndex;
        }
        public async Task<ObservableCollectionView<EscalationAndStatusThread>> QueryAllEscalationAndStatusThread(ProductWithSelectedItem AllMyPlatform, EscalationStatusWithSelectedItem EscalatonStatusList, string startDatestring,string endDatestring)
        {
            ObservableCollectionView<EscalationAndStatusThread> EscalationThreadList = new ObservableCollectionView<EscalationAndStatusThread>();
            HttpClient HttpClient = new HttpClient();
            Product MyProduct = new Product();
            MyProduct = AllMyPlatform.SelectedItem;
            string plaform = MyProduct.Platform;
            EscalationStatus MyEscalationStatus = new EscalationStatus();
            MyEscalationStatus = EscalatonStatusList.SelectedItem;
            string status = MyEscalationStatus.Status;
            var HttpResponseMessage = await HttpClient.GetAsync(new Uri(string.Format("http://escalationmanagerwebapi.azurewebsites.net/api/ethreads?etime1={0}&etime2={1}&alias={2}&platform={3}&forum={4}&status={5}", startDatestring, endDatestring, "fapeng", plaform, "", status)));
            ObservableCollection<EscalationThread> AllMyEscalationThread = new ObservableCollection<EscalationThread>();
            if (HttpResponseMessage.StatusCode == HttpStatusCode.Ok)
            {
                EscalationThreadList.Items.Clear();
                var result = await HttpResponseMessage.Content.ReadAsStringAsync();
                AllMyEscalationThread = JsonConvert.DeserializeObject<ObservableCollection<EscalationThread>>(result);
                foreach (var escalationthread in AllMyEscalationThread)
                {
                    EscalationAndStatusThread EscalationAndStatusThread = new EscalationAndStatusThread();
                    EscalationAndStatusThread.EscalationThread = escalationthread;
                    EscalationAndStatusThread.EscalationStatusList = EscalatonStatusList.MyEscalationStatusList;
                    EscalationThreadList.Items.Add(EscalationAndStatusThread);
                }
                Task<bool> tk = LogSearchCaseStatesExist(AllMyEscalationThread);
                bool a = await tk;
            }
            return EscalationThreadList;
        }

        /// <summary>
        /// 记录查询的case记录
        /// </summary>
        /// <param name="AllMyEscalationThread"></param>
        /// <returns></returns>
        private async Task<bool> LogSearchCaseStatesExist(ObservableCollection<EscalationThread> AllMyEscalationThread)
        {
            bool exists = false;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var escalationthread in AllMyEscalationThread)
                {
                    DateTime escalteDate = DateTime.Parse(escalationthread.EscalatedDatetime.ToString());
                    if (DateTime.Now.ToString("yyyy-MM-dd") == escalteDate.ToString("yyyy-MM-dd"))
                    { stringBuilder.Append(escalationthread.Url + ";"); }
                    
                    //EscalationAndStatusThread EscalationAndStatusThread = new EscalationAndStatusThread();
                    //EscalationAndStatusThread.EscalationThread = escalationthread;
                }
                if(stringBuilder.Length!=0)
                {
                    //Windows.ApplicationModel.Package.Current.InstalledLocation
                    IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
                    string filename = "NotificationRecords_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                    IStorageFile storageFile = await applicationFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                    IRandomAccessStream accessStream = await storageFile.OpenReadAsync();
                    await FileIO.WriteTextAsync(storageFile, stringBuilder.ToString());
                    exists = true;
                }
            }
            catch
            {
                exists = false;
            }
            return await Task.FromResult(exists); ;
        }

    }
}
