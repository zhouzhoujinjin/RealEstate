import { EChart } from "@kbox-labs/react-echarts";
import { FC, useEffect, useState } from "react";
import { GetYearStat } from "../services/satellite";

export const ModelStat = () => {
  const [xAxisData, setXAxisData] = useState([]);
  const [seriesData, setSeriesData] = useState([]);
  const [seriesData2, setSeriesData2] = useState([]);

  useEffect(() => {
    GetYearStat((data) => {
      setXAxisData(data.map((item) => item[0]));
      setSeriesData(data.map((item) => parseInt(item[1])));
      setSeriesData2(data.map((item) => parseInt(item[2])));
    });
  }, []);
  return (
    <div className="card">
      <h4>年度统计信息</h4>
      <EChart
        backgroundColor="transparent"
        grid={{ top: 40, left: 20, bottom: 10, right: 20, containLabel: true }}
        renderer="svg"
        theme="dark"
        legend={{
          show: true,
          data: ["计划发射", "实际发射"],
        }}
        style={{
          height: 430,
        }}
        xAxis={{
          type: "category",
          data: xAxisData,
        }}
        yAxis={{
          type: "value",
        }}
        series={[
          {
            name: "计划发射",
            data: seriesData2,
            type: "bar",
            color: '#013870'
          },
          {
            name: "实际发射",
            data: seriesData,
            type: "bar",
            color: '#9794F2'
          },
        ]}
      ></EChart>
    </div>
  );
};
