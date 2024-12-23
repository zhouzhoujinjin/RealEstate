using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstate.Utils
{
  public class CalculateDistance
  {
    private const double EARTH_RADIUS = 6378137;

    /// <summary>
    /// 计算两点位置的距离，返回两点的距离，单位 米
    /// 该公式为GOOGLE提供，误差小于0.2米
    /// </summary>
    /// <param name="lat1">第一点纬度</param>
    /// <param name="lng1">第一点经度</param>
    /// <param name="lat2">第二点纬度</param>
    /// <param name="lng2">第二点经度</param>
    /// <returns></returns>
    public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
    {
      double radLat1 = Rad(lat1);
      double radLng1 = Rad(lng1);
      double radLat2 = Rad(lat2);
      double radLng2 = Rad(lng2);
      double a = radLat1 - radLat2;
      double b = radLng1 - radLng2;
      double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
      return result;
    }

    private static double Rad(double d)
    {
      return (double)d * Math.PI / 180d;
    }
  }
}
