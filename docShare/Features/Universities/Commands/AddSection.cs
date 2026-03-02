using Application.Common;
using Application.Interfaces;
using Domain.Entities;

namespace API.Features.Universities.Commands;

public record AddSectionCommand(int UniversityId, string Name);

public class AddSectionHandler
{
    private readonly IUniversitites _repo;

    public AddSectionHandler(IUniversitites repo)
    {
        _repo = repo;
    }

    public async Task<Result<UniversitySection>> HandleAsync(AddSectionCommand cmd, CancellationToken ct = default)
    {
        if (!await _repo.HasValue(cmd.UniversityId))
            return Result<UniversitySection>.Failure("Không tìm thấy trường đại học.", 404);

        var section = await _repo.AddSectionToUniversityAsync(cmd.UniversityId, cmd.Name);
        await _repo.SaveChangeAsync();
        return Result<UniversitySection>.Success(section);
    }
}
