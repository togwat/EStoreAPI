import { useIsMobile } from "@/hooks/use-mobile";
import { listSkills, SkillSummary } from "@/api/skills";
import SkillCard from "./components/SkillCard";
import { useEffect, useState } from "react";

export default function SkillsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [skillSummaries, setSkillSummaries] = useState<SkillSummary[]>([]);

    useEffect(() => {
        listSkills().then(setSkillSummaries);
    }, []);

    const toCard = (skillSummary: SkillSummary) => (
        <SkillCard
            key={skillSummary.name}
            skillSummary={skillSummary}
            onClick={() => {}}
        />
    );

    return (
        <div>
            <div className={isMobile ? "" : "p-8"}>
                { !isMobile && <h1 className="pb-4">{title}</h1> }
                <span className="pl-1 text-muted-foreground">{skillSummaries.length} skills</span>
                {/** desktop grid, mobile rows */}
                <div className={isMobile
                    ? "py-4 flex flex-col gap-2"
                    : "py-4 grid grid-cols-[repeat(auto-fill,_16rem)] gap-4"}>
                    {skillSummaries.map(toCard)}
                </div>
            </div>
        </div>
    )
}