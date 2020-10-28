﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using BLE202.ViewModels;
using Xamarin.Forms;

namespace BLE202.Droid
{
    class BleServer
    {
        private readonly BluetoothManager _bluetoothManager;
        private BluetoothAdapter _bluetoothAdapter;
        private BleGattServerCallback _bluettothServerCallback;
        private BluetoothGattServer _bluetoothServer;
        private BluetoothGattCharacteristic _characteristic;
        private BleEventArgs et;
        public BleServer(Context ctx)
        {          
            _bluetoothManager = (BluetoothManager)ctx.GetSystemService(Context.BluetoothService);
            _bluetoothAdapter = _bluetoothManager.Adapter;

            _bluettothServerCallback = new BleGattServerCallback();
            _bluetoothServer = _bluetoothManager.OpenGattServer(ctx, _bluettothServerCallback);          
            var service = new BluetoothGattService(UUID.FromString("ffe0ecd2-3d16-4f8d-90de-e89e7fc396a5"),
                GattServiceType.Primary);
            _characteristic = new BluetoothGattCharacteristic(UUID.FromString("d8de624e-140f-4a22-8594-e2216b84a5f2"), GattProperty.Read | GattProperty.Notify | GattProperty.Write, GattPermission.Read | GattPermission.Write);
            _characteristic.AddDescriptor(new BluetoothGattDescriptor(UUID.FromString("28765900-7498-4bd4-aa9e-46c4a4fb7b07"),
                    GattDescriptorPermission.Read | GattDescriptorPermission.Write));

            service.AddCharacteristic(_characteristic);

            _bluetoothServer.AddService(service);
            _bluettothServerCallback.CharacteristicReadRequest += _bluettothServerCallback_CharacteristicReadRequest;
            _bluettothServerCallback.DescriptorWriteRequest += _bluettothServerCallback_DescriptorWriteRequest;
            _bluettothServerCallback.CharacteristicWriteRequest += _bluettothServerCallback_CharacteristicWriteRequest;
            //           _bluettothServerCallback.NotificationSent += _bluettothServerCallback_NotificationSent;
            //           MessagingCenter.Send<BLE202.App, string>((BLE202.App)Xamarin.Forms.Application.Current, "Hi", "Server created!");
            _bluetoothAdapter.SetName("Concungungoc");
            BluetoothLeAdvertiser myBluetoothLeAdvertiser = _bluetoothAdapter.BluetoothLeAdvertiser;
            var builder = new AdvertiseSettings.Builder();
            builder.SetAdvertiseMode(AdvertiseMode.LowLatency);
            builder.SetConnectable(true);
            builder.SetTimeout(0);
            builder.SetTxPowerLevel(AdvertiseTx.PowerHigh);
            AdvertiseData.Builder dataBuilder = new AdvertiseData.Builder();
            dataBuilder.SetIncludeDeviceName(true);
            //dataBuilder.AddServiceUuid(ParcelUuid.FromString("ffe0ecd2-3d16-4f8d-90de-e89e7fc396a5"));
            dataBuilder.SetIncludeTxPowerLevel(true);

            myBluetoothLeAdvertiser.StartAdvertising(builder.Build(), dataBuilder.Build(), new BleAdvertiseCallback());
        }
        public void SetupMesss()
        {
            MessagingCenter.Subscribe<App, string>((App)global::Xamarin.Forms.Application.Current, "GetValuex", async (sender, arg) => {
                  if (et != null)
                  {
                      et.Characteristic.SetValue(arg.ToString());
                      MessagingCenter.Send<BLE202.App, string>((BLE202.App)Xamarin.Forms.Application.Current, "Hi", arg.ToString());
                      _bluetoothServer.SendResponse(et.Device, et.RequestId, GattStatus.Success, et.Offset, et.Characteristic.GetValue());
                  }
            });
        }
        private int _count = 0;
        private Stopwatch _sw = new Stopwatch();

        void _bluettothServerCallback_NotificationSent(object sender, BleEventArgs e)
        {
            /*
            if (_count == 0)
            {
                _sw = new Stopwatch();
                _sw.Start();
            }

            if (_count < 1000)
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new System.Random();
                var result = new string(
                    Enumerable.Repeat(chars, 20)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray());
                _characteristic.SetValue(result);
                MessagingCenter.Send<BLE202.App, string>((BLE202.App)Xamarin.Forms.Application.Current, "Hi", result);
                _bluetoothServer.NotifyCharacteristicChanged(e.Device, _characteristic, false);

                _count++;

            }
            else
            {
                _sw.Stop();
                Console.WriteLine("Sent # {0} notifcations. Total kb:{2}. Time {3}(s). Throughput {1} bytes/s", _count,
                    _count * 20.0f / _sw.Elapsed.TotalSeconds, _count * 20 / 1000, _sw.Elapsed.TotalSeconds);
            } */
         //   _characteristic.SetValue("hello");
          //  MessagingCenter.Send<BLE202.App, string>((BLE202.App)Xamarin.Forms.Application.Current, "Hi", "hello");
         //   _bluetoothServer.NotifyCharacteristicChanged(e.Device, _characteristic, false);
        } 

        private bool _notificationsStarted = false;

        private int _readRequestCount = 0;
        void _bluettothServerCallback_CharacteristicReadRequest(object sender, BleEventArgs e)
        {
            _readRequestCount++;
            e.Characteristic.SetValue(String.Format("Right on {0}!", _readRequestCount));
            MessagingCenter.Send<BLE202.App, string>((BLE202.App)Xamarin.Forms.Application.Current, "Hi", String.Format("Right on {0}!", _readRequestCount));
            _bluetoothServer.SendResponse(e.Device, e.RequestId, GattStatus.Success, e.Offset, e.Characteristic.GetValue());


 //           _bluetoothServer.NotifyCharacteristicChanged(e.Device, e.Characteristic, false);
        }
        void _bluettothServerCallback_DescriptorWriteRequest(object sender, BleEventArgs e)
        {
            e.Characteristic.SetValue(String.Format("Thanks for message"));
            string result = System.Text.Encoding.UTF8.GetString(e.Value);
            MessagingCenter.Send<BLE202.App, string>((BLE202.App)Xamarin.Forms.Application.Current, "Hi", "[Read]: " + result);
            _bluetoothServer.SendResponse(e.Device, e.RequestId, GattStatus.Success, e.Offset, e.Characteristic.GetValue());
        }
        void _bluettothServerCallback_CharacteristicWriteRequest(object sender, BleEventArgs e)
        {
            e.Characteristic.SetValue(String.Format("Thanks for message"));
            string result = System.Text.Encoding.UTF8.GetString(e.Value);
            MessagingCenter.Send<BLE202.App, string>((BLE202.App)Xamarin.Forms.Application.Current, "Hi", "[Read]: " + result);
            _bluetoothServer.SendResponse(e.Device, e.RequestId, GattStatus.Success, e.Offset, e.Characteristic.GetValue());
        }

        void _bluettothServerCallback_ConnectionStateChange(object sender, BleEventArgs e)
        {
            et = e;
            MessagingCenter.Send<BLE202.App, string>((BLE202.App)Xamarin.Forms.Application.Current, "Hi", "New Device Connect ");

        }


    }

    public class BleAdvertiseCallback : AdvertiseCallback
    {
        public override void OnStartFailure(AdvertiseFailure errorCode)
        {
            Console.WriteLine("Adevertise start failure {0}", errorCode);
            base.OnStartFailure(errorCode);
        }
        public ServerViewModel SharedContext { get; set; }
        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            Xamarin.Forms.MessagingCenter.Send("hahahahah", "eventName");
            Console.WriteLine("Adevertise start success {0}", settingsInEffect.Mode);
            base.OnStartSuccess(settingsInEffect);
        }
    }
}