import { Job, getJobs } from "@/api/jobs";
import { ChartContainer, ChartTooltip, ChartTooltipContent, type ChartConfig } from "@/components/ui/chart";
import { Bar, BarChart, CartesianGrid, XAxis } from "recharts";
import { useEffect, useState } from "react";

// 1 month chart range
const startDate = new Date();
startDate.setMonth(startDate.getMonth() - 1);
const endDate = new Date();

// format dates in range to chartData string
function generateDateRange(start: Date, end: Date): Record<string, number> {
    const dates: Record<string, number> = {};
    const current = new Date(start);

    while (current <= end) {
        // YYYY-MM-DD
        const dateString = current.toISOString().slice(0, 10);
        dates[dateString] = 0;

        // increment date
        current.setDate(current.getDate() + 1);
    }

    return dates;
}

function buildChartData(jobs: Job[]) {
    const dateCounts = generateDateRange(startDate, endDate);

    for (const job of jobs) {
        const receiveTime = job.receiveTime;
        // extract only the date, act as key
        const date = new Date(receiveTime).toISOString().slice(0, 10);

        if (date in dateCounts) {
            dateCounts[date]++;
        }
    }

    // convert to array
    return Object.entries(dateCounts).map(([date, jobs]) => ({ date, jobs }));
}

const chartConfig: ChartConfig = {
    jobs: {
        label: "Jobs",
        color: "var(--color-primary)"
    }
}

export function JobIntakeChart() {
    const [chartData, setChartData] = useState<{ date: string; jobs: number }[]>([]);

    useEffect(() => {
        getJobs().then(jobs => setChartData(buildChartData(jobs)));
    }, []);

    return (
        <div className="flex flex-col gap-2">
            <h2 className="font-medium">Job Intake Per Day</h2>
            <ChartContainer config={chartConfig} className="aspect-auto max-w-2xl h-40">
                <BarChart data={chartData}>
                    <Bar dataKey="jobs" fill="var(--color-jobs)" />
                    <CartesianGrid vertical={false} />
                    <XAxis
                        dataKey="date"
                        tickLine={false}
                        axisLine={false}
                        tickMargin={8}
                        minTickGap={32}
                        tickFormatter={(value) => {
                            const date = new Date(value);
                            return date.toLocaleDateString("en-NZ", {
                                month: "short",
                                day: "numeric",
                            });
                        }}
                    />
                    <ChartTooltip 
                        content={
                            <ChartTooltipContent
                                labelFormatter={(value) => {
                                    return new Date(value).toLocaleDateString("en-NZ", {
                                        month: "short",
                                        day: "numeric",
                                        year: "numeric",
                                    })
                                }}
                            />
                        }
                    />
                </BarChart>
            </ChartContainer>
        </div>
    )
}
