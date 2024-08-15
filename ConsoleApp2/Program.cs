using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AdministrativeDataApp
{
    public class Province
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<District> Districts { get; set; } = new List<District>();
    }

    public class District
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<Ward> Wards { get; set; } = new List<Ward>();
    }

    public class Ward
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class Program
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task Main(string[] args)
        {
            // Lấy danh sách tất cả các tỉnh, thành phố
            var provinces = await GetProvinces();

            // Lấy danh sách quận, huyện và xã, phường cho từng tỉnh, thành phố
            foreach (var province in provinces)
            {
                var districts = await GetDistrictsByProvince(province.Code);
                foreach (var district in districts)
                {
                    var wards = await GetWardsByDistrict(district.Code);
                    district.Wards.AddRange(wards);
                }
                province.Districts.AddRange(districts);
            }

            // Chuyển đổi dữ liệu thành JSON
            string jsonResult = JsonConvert.SerializeObject(provinces, Formatting.Indented);

            // Lưu vào file JSON
            System.IO.File.WriteAllText("VietnamAdministrativeData.json", jsonResult);

            Console.WriteLine("Data has been saved to VietnamAdministrativeData.json");
        }

        private static async Task<List<Province>> GetProvinces()
        {
            var response = await client.GetStringAsync("https://api.mysupership.vn/v1/partner/areas/province");
            var provinces = JsonConvert.DeserializeObject<ResponseWrapper<List<Province>>>(response)?.Results;
            return provinces ?? new List<Province>();
        }

        private static async Task<List<District>> GetDistrictsByProvince(string provinceCode)
        {
            var response = await client.GetStringAsync($"https://api.mysupership.vn/v1/partner/areas/district?province={provinceCode}");
            var districts = JsonConvert.DeserializeObject<ResponseWrapper<List<District>>>(response)?.Results;
            return districts ?? new List<District>();
        }

        private static async Task<List<Ward>> GetWardsByDistrict(string districtCode)
        {
            var response = await client.GetStringAsync($"https://api.mysupership.vn/v1/partner/areas/commune?district={districtCode}");
            var wards = JsonConvert.DeserializeObject<ResponseWrapper<List<Ward>>>(response)?.Results;
            return wards ?? new List<Ward>();
        }

        public class ResponseWrapper<T>
        {
            public string Status { get; set; } = null!;
            public string Message { get; set; } = null!;
            public T Results { get; set; } = default!;
        }
    }
}
