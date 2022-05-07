using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Qmmands;

using DescAttribute = System.ComponentModel.DescriptionAttribute;

namespace Slqsh.Test;

public enum Order
{
    [Desc("This should be first.")]
    First,
    [Desc("This should be second.")]
    Second,
    [Desc("This should be third.")]
    Third,
    [Desc("This should be fourth.")]
    Fourth
}

public class SampleModule : SlashModuleBase
{
    [Command("echo")]
    [Description("Echo some text back to yourself.")]
    public SlashCommandResult Echo(
        [Description("The text you want to hear.")]
            string text)
    {
        return Response(text);
    }

    [Command("color")]
    [Description("Return an embed with a specified color by name.")]
    public SlashCommandResult Color(
        [Description("The color you wish to see.")] 
        [AutoComplete]
            Color color)
    {
        return Response(new LocalEmbed().WithColor(color).WithDescription($"You picked: #{color.RawValue:X6}"));
    }

    [Command("menu")]
    [Description("Display a sample menu.")]
    public SlashCommandResult ExampleMenu()
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyz";
        var pages = alphabet.Select(x => new Page()
            .WithEmbeds(new LocalEmbed()
                .WithColor(Disqord.Color.Random)
                .WithTitle("What's your favorite letter of the alphabet?")
                .WithDescription(x.ToString())));

        return Pages(pages);
    }

    [Command("choice")]
    [Description("Make a choice.")]
    public SlashCommandResult Choice(
        [Description("The choice to select.")]
            Order order)
    {
        return Response($"You picked: {order}.");
    }

    [Command("choice2")]
    [Description("Make a slightly different choice.")]
    public SlashCommandResult Choice2(
        [Description("Your favorite number.")]
        [Choice("1", 1)]
        [Choice("2", 2)]
        [Choice("3", 3)]
        [Choice("4", 4)]
        [Choice("5", 5)]
        [Choice("Something else", int.MaxValue)]
            int number)
    {
        if (number == int.MaxValue)
            return Response("You must like very large numbers.");

        return Response($"Your favorite number is: {number}.");
    }

    [Command("choice3")]
    [Description("Make a choice - or don't.")]
    public SlashCommandResult Choice3(
        [Description("The choice to select, or none at all.")]
            Order? order = null)
    {
        if (!order.HasValue)
            return Response("You picked nothing.");

        return Response($"You picked: {order}.");
    }

    [Command("choice4")]
    [Description("Make a limited set of choices.")]
    public SlashCommandResult Choice4(
        [Description("One of two choices.")]
        [Choice("The first choice.", Order.First)]
        [Choice("The second choice.", Order.Second)]
            Order order)
    {
        if (order is Order.Third or Order.Fourth)
            throw new Exception();

        return Response($"You picked: {order}.");
    }

    [Command("choice5")]
    [Description("Make an intentionally restricted set of choices.")]
    public SlashCommandResult Choice5(
        [Description("The choice to select.")] 
        [ExcludeEnumValues(Order.First, Order.Second)]
            Order order)
    {
        return Response($"You picked: {order}.");
    }

    [Command("modal")]
    [Description("Sends a simple modal and waits for your response.")]
    [RunMode(RunMode.Parallel)]
    public async Task<SlashCommandResult> ModalAsync()
    {
        var modal = await Modal("Sample Modal", new LocalTextInputComponent()
            .WithLabel("Hello hi")
            .WithStyle(TextInputComponentStyle.Short)
            .WithIsRequired()
            .WithPlaceholder("Say something here"));
        //var modal = await Modal("Sample Modal", "What would you like to say?", TextInputComponentStyle.Paragraph);

        if (modal is null)
            return Followup("You didn't fill it out in time.");

        var firstRow = (IRowComponent) modal.Components[0];
        var component = (ITextInputComponent) firstRow.Components[0];

        await modal.Response()
            .SendMessageAsync(new LocalInteractionMessageResponse().WithContent($"You submitted: {component.Value}"));

        return default;

        // return Followup($"You submitted: {component.Value}");
    }
}