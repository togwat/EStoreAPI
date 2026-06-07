import { Job, getJobs } from "@/api/jobs";
import { ChartContainer, ChartTooltip, ChartTooltipContent, type ChartConfig } from "@/components/ui/chart";
import { Area, AreaChart, CartesianGrid, XAxis } from "recharts";
import { useEffect, useState } from "react";
import { formatPrice } from "@/lib/formatPrice";

// 2 month chart range
const startDate = new Date();
startDate.setMonth(startDate.getMonth() - 2);
const endDate = new Date();

// Returns the ISO date string of the Sunday ending the week that contains `date`
function getWeekEndingSunday(date: Date): string {
    const d = new Date(date);
    const day = d.getDay(); // 0=Sun ... 6=Sat
    d.setDate(d.getDate() + (day === 0 ? 0 : 7 - day));
    return d.toISOString().slice(0, 10);
}

// Generates one entry per Mon–Sun week between start and end, keyed by Sunday date
function generateWeekRange(start: Date, end: Date): Record<string, number> {
    const weeks: Record<string, number> = {};

    // Rewind to Monday of the week containing start
    const current = new Date(start);
    const day = current.getDay();
    current.setDate(current.getDate() + (day === 0 ? -6 : 1 - day));

    while (current <= end) {
        const sunday = new Date(current);
        sunday.setDate(sunday.getDate() + 6);
        weeks[sunday.toISOString().slice(0, 10)] = 0;
        current.setDate(current.getDate() + 7);
    }

    return weeks;
}

function buildChartData(jobs: Job[]) {
    const weekTakings = generateWeekRange(startDate, endDate);

    for (const job of jobs) {
        if (!job.pickupTime || job.collectedPrice == null) continue;

        const weekKey = getWeekEndingSunday(new Date(job.pickupTime));
        if (weekKey in weekTakings) {
            weekTakings[weekKey] += parseFloat(job.collectedPrice);
        }
    }

    return Object.entries(weekTakings).map(([week, takings]) => ({ week, takings }));
}

const chartConfig: ChartConfig = {
    takings: {
        label: "Takings",
        color: "var(--color-primary)",
    },
}

export function TakingsPerWeek() {
    const [chartData, setChartData] = useState<{ week: string; takings: number }[]>([]);

    useEffect(() => {
        getJobs().then(jobs => setChartData(buildChartData(jobs)));
    }, []);

    return (
        <div className="flex flex-col gap-2">
            <h2 className="font-medium">Takings Per Week</h2>
            <ChartContainer config={chartConfig} className="aspect-auto max-w-2xl h-40">
                <AreaChart data={chartData} margin={{ left: 12, right: 12 }}>
                    <CartesianGrid vertical={false} />
                    <XAxis
                        dataKey="week"
                        tickLine={false}
                        axisLine={false}
                        tickMargin={8}
                        tickFormatter={(value) =>
                            new Date(value).toLocaleDateString("en-NZ", { month: "short", day: "numeric" })
                        }
                    />
                    <ChartTooltip
                        cursor={false}
                        content={
                            <ChartTooltipContent
                                labelFormatter={(value) =>
                                    `Week ending ${new Date(value).toLocaleDateString("en-NZ", {
                                        month: "short",
                                        day: "numeric",
                                        year: "numeric",
                                    })}`
                                }
                            />
                        }
                    />
                    <defs>
                        <linearGradient id="fillTakings" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="var(--color-takings)" stopOpacity={0.8} />
                            <stop offset="95%" stopColor="var(--color-takings)" stopOpacity={0.1} />
                        </linearGradient>
                    </defs>
                    <Area
                        dataKey="takings"
                        type="linear"
                        fill="url(#fillTakings)"
                        fillOpacity={0.4}
                        stroke="var(--color-takings)"
                        dot={true}
                        label={(props) => {
                            if (props.index !== chartData.length - 1) return <div />;
                            return (
                                // label for the last data point only
                                <text
                                    x={props.x}
                                    y={Number(props.y) - 10}
                                    textAnchor="end"
                                    className="text-xs font-medium font-mono text-foreground"
                                >
                                    {formatPrice(Number(props.value))}
                                </text>
                            );
                        }}
                    />
                </AreaChart>
            </ChartContainer>
        </div>
    );
}
