import dayjs from "dayjs";
import { useEffect, useState } from "react";

export const TemperatureHumidity = () => {
  const [temperature, setTemperature] = useState(null);
  const [humidity, setHumidity] = useState(null);
  useEffect(() => {
    const id = setInterval(() => {
      setTemperature((Math.random() * 10 + 20).toFixed(2));
      setHumidity((Math.random() * 10 + 30).toFixed(2));
    }, 5000);
    setTemperature((Math.random() * 10 + 20).toFixed(2));
    setHumidity((Math.random() * 10 + 30).toFixed(2));
    return () => clearInterval(id);
  }, []);

  return (
    <>
      温度：
      <span>{temperature}</span>℃ / 湿度：
      <span>{humidity}</span>%
    </>
  );
};
