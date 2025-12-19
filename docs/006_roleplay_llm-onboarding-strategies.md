# FreeManager: Team Discussion - LLM Onboarding Strategies

> Roleplay transcript exploring ways to effectively onboard LLM assistants to the FreeManager project.

**Date:** Current Session  
**Participants:** [Architect], [Blazor], [Data], [API], [Quality], [Sanity]

---

## Opening

**[Architect]:** Team, we need to establish a reliable way to onboard new LLM chat sessions to this project. We have comprehensive documentation now, but we need to figure out the best prompt strategy to get an assistant up to speed quickly and working effectively.

**[Sanity]:** Before we dive in, let's be clear on the goal. We want an assistant to:
1. Understand the two-codebase structure
2. Know the coding conventions
3. Know current progress and what's next
4. Be able to execute tasks without constant hand-holding

**[Architect]:** Exactly. Let's brainstorm different approaches.

---

## Approach 1: Sequential File Reading

**[Data]:** The simplest approach is to list all the docs in order:

```
Read these files in full, in order:
1. docs/001_index_quickstart-and-documentation-guide.md
2. docs/000_guide_development-patterns-and-conventions.md
3. docs/004_status_implementation-progress-and-checklist.md
4. docs/005_reference_csharp-style-guide.md
5. docs/002_plan_project-vision-and-decisions.md
6. docs/003_reference_system-architecture-and-data-model.md

Then the task is: [task here]
```

**[Quality]:** That's thorough but potentially slow. That's 6 files, over 1,300 lines of documentation. Token-heavy.

**[Blazor]:** And does the assistant actually need ALL of that for most tasks? If I'm just building a UI component, do I need the full ERD?

**[Sanity]:** Good point. We should tier the information.

---

## Approach 2: Tiered Reading Based on Task Type

**[Architect]:** What if we have different prompts for different task types?

**For UI Tasks:**
```
Read docs/001 (quickstart), docs/000 (dev guide), docs/004 (status).
Reference docs/005 (style guide) when writing code.
Look at FreeCRM-main/CRM.Client/ for Blazor patterns.

Task: Build the project list page
```

**For Backend Tasks:**
```
Read docs/001 (quickstart), docs/000 (dev guide), docs/004 (status), docs/003 (architecture).
Reference docs/005 (style guide) when writing code.
Look at FreeCRM-main/CRM.DataAccess/ for data patterns.

Task: Add a new FM_ method
```

**[API]:** I like that. Focused context for focused tasks.

**[Quality]:** But it requires the human to know which tier applies. What if they just want to hand off a task?

---

## Approach 3: Self-Discovery Prompt

**[Data]:** What if we tell the assistant to figure it out?

```
You are working on FreeManager, a Blazor WebAssembly app built on FreeCRM.

Start by reading docs/001_index_quickstart-and-documentation-guide.md - it will tell you 
what other files to read and in what order based on what you need.

For any architecture, style, or structure questions, reference FreeCRM-main/ as the 
canonical source of patterns.

The task is: [task here]
```

**[Architect]:** That's elegant. The quickstart file becomes the entry point, and it has the reading order built in.

**[Blazor]:** Plus the quickstart already has the "Code Style Quick Reference" table for the most common rules. The assistant can reference the full style guide only when needed.

**[Sanity]:** I like this. It's meta - we're using our documentation to document how to use our documentation.

---

## Approach 4: Compressed Context Prompt

**[Quality]:** For token-constrained situations, what about a compressed version?

```
FreeManager: Blazor WASM app builder on FreeCRM framework (.NET 10)

Two codebases:
- Root (CRM/, CRM.Client/, etc.) = working copy, modify this
- FreeCRM-main/ = stock reference, read-only, use for patterns

Conventions:
- FM_ prefix for all FreeManager methods
- .App.FreeManager.cs suffix for extension files
- File size: 300 ideal, 600 max
- Style: explicit types, _camelCase fields, same-line braces for control flow

Status: Phase 1 (backend) complete, Phase 2 (UI) next

Full docs: docs/001_index_quickstart-and-documentation-guide.md

Task: [task here]
```

**[API]:** That's maybe 150 tokens for the core context. Very efficient.

**[Architect]:** But it assumes the human knows enough to write that. We need a copy-paste solution.

---

## Approach 5: Hybrid - Quick Context + Deep Dive

**[Sanity]:** What if we combine approaches? Start compressed, then let the assistant deep-dive as needed?

```
FreeManager is a Blazor WebAssembly application builder built on the FreeCRM framework.

CRITICAL FIRST STEP: Read docs/001_index_quickstart-and-documentation-guide.md in full.
This file explains the project structure, two-codebase setup, documentation system, 
and tells you which other docs to read for your specific needs.

Key conventions (details in docs/005):
- FM_ prefix for FreeManager code
- .App.FreeManager.cs pattern for extensions  
- FreeCRM-main/ is read-only reference for patterns
- File size limit: 600 lines max

Current status: Phase 1 complete, Phase 2 (Editor UI) is next.
See docs/004 for detailed checklist.

The task is: [task here]
```

**[Architect]:** This gives immediate context but points to deeper resources.

**[Blazor]:** And it emphasizes the quickstart as the single entry point.

**[Quality]:** The assistant can then choose to read more docs based on task complexity.

---

## Discussion: Best Approach

**[Architect]:** Let's evaluate. Which approach works best for our needs?

**[Data]:** Approach 1 (sequential) is too heavy for most tasks.

**[API]:** Approach 2 (tiered) requires too much human judgment.

**[Blazor]:** Approach 3 (self-discovery) is elegant but might be too open-ended.

**[Quality]:** Approach 4 (compressed) is efficient but loses nuance.

**[Sanity]:** Approach 5 (hybrid) seems like the sweet spot. It gives:
- Immediate critical context (two codebases, conventions)
- A clear entry point (001 quickstart)
- Pointers to deeper resources (004 status, 005 style)
- Flexibility for the assistant to dig deeper as needed

**[Architect]:** Agreed. Let's refine Approach 5 as our recommended prompt.

---

## Recommended Prompt Template

**[Architect]:** Here's our final recommended prompt for starting a new session:

```
FreeManager is a Blazor WebAssembly application builder built on the FreeCRM framework.

FIRST: Read docs/001_index_quickstart-and-documentation-guide.md in full - it explains 
the project structure, two-codebase setup, and documentation system.

THEN: Read docs/000_guide_development-patterns-and-conventions.md for FreeManager-specific 
patterns (FM_ prefix, .App. pattern, team roles).

THEN: Read docs/004_status_implementation-progress-and-checklist.md to understand current 
progress and what tasks are next.

WHEN WRITING CODE: Follow docs/005_reference_csharp-style-guide.md conventions.

FOR ARCHITECTURE/STYLE QUESTIONS: Reference FreeCRM-main/ as the source of inspiration.

Current status: Phase 1 (Data Foundation) is complete. Phase 2 (Editor UI) is next.

The task is: [describe task here]
```

**[Quality]:** Should we add anything about asking the CTO for decisions?

**[Sanity]:** Yes. Add this:

```
If you encounter a design decision that requires approval (new dependencies, architecture 
changes, scope questions), stop and ask before proceeding.
```

---

## Secondary Topic: Resuming Work After Gap

**[Blazor]:** What about resuming work in a new session? The assistant won't remember previous context.

**[Data]:** The 004_status doc should be the source of truth. We need to keep it updated.

**[API]:** And we could create session-specific docs like `007_standup_YYYY-MM-DD.md` to track what was done.

**[Architect]:** Good idea. After each significant work session, we update 004_status and optionally create a standup note.

**[Sanity]:** The resume prompt would then be:

```
Continuing work on FreeManager.

Read docs/001 for project overview, then docs/004 for current status.
[Optional: Read docs/007_standup_YYYY-MM-DD.md for last session's progress]

Continue with: [next task or "pick up where we left off"]
```

---

## Action Items

**[Architect]:** Summary of decisions:

1. **Primary onboarding prompt:** Hybrid approach (Approach 5) - immediate context + deep-dive pointers
2. **Entry point:** 001_index is the single starting point
3. **Pattern reference:** FreeCRM-main/ is canonical for architecture/style questions
4. **Status tracking:** 004_status is source of truth, update after each session
5. **Decision escalation:** Ask CTO for approval on dependencies, architecture, scope

**[Sanity]:** Questions for CTO:

1. **Approve the recommended prompt template?**
2. **Approve starting Phase 2 (Editor UI)?**
3. **BlazorMonaco NuGet package** - approve adding this dependency for the code editor?
4. **File organization for Blazor components** - should FM components go in `CRM.Client/Shared/AppComponents/` or a new folder?

---

## Summary

**[Summary]:**

The team discussed five approaches to onboarding LLM assistants:
1. Sequential file reading (too heavy)
2. Tiered by task type (requires human judgment)
3. Self-discovery (too open-ended)
4. Compressed context (loses nuance)
5. **Hybrid** (RECOMMENDED) - immediate context + pointers to deep resources

The hybrid approach was selected because it:
- Gives critical context immediately
- Points to 001_index as the single entry point
- Allows flexible depth based on task needs
- Emphasizes FreeCRM-main as pattern reference

A CTO briefing document (007) will be prepared with the recommended prompt, current status, and decision requests.
