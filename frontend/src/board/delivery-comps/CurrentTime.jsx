import dayjs from "dayjs"
import { useEffect, useState } from "react"

export const CurrentTime = () => {
    const [time, setTime] = useState('')

    useEffect(() => {
        const id = setInterval(() => {
            setTime(dayjs().format('YYYY-MM-DD HH:mm:ss'))
        }, 1000)
        return () => clearInterval(id)
    })

    return <div>{time}</div>
}