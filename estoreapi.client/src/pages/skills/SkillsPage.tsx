import { useIsMobile } from "@/hooks/use-mobile";
import { getSkill, listSkills, Skill, SkillSummary } from "@/api/skills";
import SkillCard from "./components/SkillCard";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { ArrowLeft, DownloadIcon, PencilIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import Markdown from "react-markdown";
import remarkGfm from "remark-gfm";
import remarkFrontmatter from "remark-frontmatter";
import { downloadTextFile } from "@/lib/downloadTextFile";

export default function SkillsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [summaries, setSummaries] = useState<SkillSummary[]>([]);
    const [selectedSummary, setSelectedSummary] = useState<SkillSummary | null>(null);
    const [selectedSkill, setSelectedSkill] = useState<Skill | null>(null);

    useEffect(() => {
        listSkills().then(setSummaries);
    }, []);

    // use selectedSummary to switch to viewing/editing mode immediately,
    // and wait for content fetching there
    useEffect(() => {
        if (!selectedSummary) return;

        setSelectedSkill(null);

        getSkill(selectedSummary.name)
            .then((skill) => { setSelectedSkill(skill); })
            .catch(() => { setSelectedSummary(null); });
    }, [selectedSummary]);

    const toCard = (skillSummary: SkillSummary) => (
        <SkillCard
            key={skillSummary.name}
            skillSummary={skillSummary}
            onClick={() => setSelectedSummary(skillSummary)}
        />
    );

    return (
        <div>
            {selectedSummary ? (
                <div className={cn("flex flex-col gap-4", isMobile ? "" : "p-8")}>
                    {/** skill viewing/editing page */}
                    {/** header */}
                    <div className={cn("flex items-center justify-between", !isMobile && "pt-4")}>
                        <div className="flex items-center gap-2">
                            <Button variant="outline" size="icon" onClick={() => setSelectedSummary(null)}><ArrowLeft /></Button>
                            <h2 className="text-xl font-semibold">{selectedSummary.name}</h2>
                        </div>

                        <div className="flex items-center gap-2">
                            <Button
                                size={isMobile ? "icon" : "lg"}
                                variant="outline"
                                disabled={!selectedSkill}
                                onClick={() => selectedSkill && downloadTextFile(`${selectedSkill.name}.md`, selectedSkill.file, "text/markdown")}
                            ><DownloadIcon />{!isMobile && "Download"}</Button>
                            <Button size={isMobile ? "icon" : "lg"} onClick={() => {}}><PencilIcon />{!isMobile && "Edit"}</Button>
                        </div>
                    </div>
                    {/** markdown renderer/text editor */}
                    <p className="px-1">{selectedSummary.summary}</p>
                    {/** first part is heading & list styles, 2nd part is container style */}
                    {selectedSkill && (
                        <div className="
                        [&_h1]:mb-2 [&_h1]:text-2xl [&_h1]:font-semibold [&_h2]:mt-6 [&_h2]:mb-2 [&_h2]:text-xl [&_h2]:font-semibold [&_h3]:mt-4 [&_h3]:mb-1 [&_h3]:text-lg [&_h3]:font-medium [&_p]:my-3 [&_ul]:my-3 [&_ul]:ml-6 [&_ul]:list-disc [&_ol]:my-3 [&_ol]:ml-6 [&_ol]:list-decimal [&_li]:my-1 [&_a]:text-primary [&_a]:underline [&_code]:rounded [&_code]:bg-muted [&_code]:px-1 [&_code]:py-0.5 [&_code]:text-sm [&_pre]:my-3 [&_pre]:overflow-x-auto [&_pre]:rounded-lg [&_pre]:bg-muted [&_pre]:p-3 [&_blockquote]:border-l-2 [&_blockquote]:pl-3 [&_blockquote]:text-muted-foreground

                        border bg-input rounded-xl p-4 max-w-3xl mx-auto my-2
                        ">
                            <Markdown remarkPlugins={[remarkGfm, remarkFrontmatter]}>
                                {selectedSkill.file}
                            </Markdown>
                        </div>
                    )}
                </div>
            ) : (
                <div className={isMobile ? "" : "p-8"}>
                    {/** skill cards grid/list */}
                    {/** header */}
                    { !isMobile && <h1 className="pb-4">{title}</h1> }
                    <span className="pl-1 text-muted-foreground">{summaries.length} skills</span>
                    {/** desktop grid, mobile rows */}
                    <div className={isMobile
                        ? "py-4 flex flex-col gap-2"
                        : "py-4 grid grid-cols-[repeat(auto-fill,_16rem)] gap-4"}>
                        {summaries.map(toCard)}
                    </div>
                </div>
            )}
        </div>
    )
}