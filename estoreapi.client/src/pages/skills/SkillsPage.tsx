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
            <div className={isMobile ? "px-8" : "p-8"}>
                { !isMobile && <h1 className="pb-4">{title}</h1> }
                <div>
                    {skillSummaries.map(toCard)}
                </div>
            </div>
        </div>
    )
}