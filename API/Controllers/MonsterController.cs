﻿using System.Globalization;
using API.Models;
using CsvHelper;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MonsterController : BaseApiController
{
    private readonly IBattleOfMonstersRepository _repository;

    public MonsterController(IBattleOfMonstersRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Find(int id)
    {
        var monster = await _repository.Monsters.FindAsync(id);

        if (monster == null)
        {
            return NotFound($"The monster with ID = {id} not found.");
        }
        return Ok(monster);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Add([FromBody] Monster monster)
    {
        await _repository.Monsters.AddAsync(monster);
        await _repository.Save();
        return Ok(monster);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Remove(int id)
    {
        var existingMonster = await _repository.Monsters.FindAsync(id);

        if (existingMonster == null)
        {
            return NotFound($"The monster with ID = {id} not found.");
        }

        await _repository.Monsters.RemoveAsync(id);
        await _repository.Save();
        return Ok();
    }

    [HttpPost("import-csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ImportCsv(IFormFile file)
    {
        try
        {
            string ext = Path.GetExtension(file.FileName);
            string filename = Guid.NewGuid().ToString() + ext;
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
            string filepath = Path.Combine(directory, filename);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using (FileStream fs = System.IO.File.Create(filepath))
            {
                await file.CopyToAsync(fs);
            }

            if (ext != ".csv")
            {
                return BadRequest("The extension is not supporting.");
            }

            using (var reader = new StreamReader(filepath))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    try
                    {
                        var records = csv.GetRecords<MonsterToImport>().ToList();
                        var monsters = records.Select(x => new Monster()
                        {
                            Name = x.name,
                            Attack = x.attack,
                            Defense = x.defense,
                            Speed = x.speed,
                            Hp = x.hp,
                            ImageUrl = x.imageUrl
                        });

                        await _repository.Monsters.AddAsync(monsters);
                        await _repository.Save();

                        csv.Dispose();
                        reader.Dispose();


                        System.IO.File.Delete(filepath);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        csv.Dispose();
                        reader.Dispose();

                        System.IO.File.Delete(filepath);
                        return BadRequest("Wrong data mapping.");
                    }
                }
            }
        }
        catch (Exception)
        {
            return BadRequest("Wrong data mapping.");
        }
    }
}
