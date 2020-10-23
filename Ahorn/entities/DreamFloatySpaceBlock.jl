module CommunalHelperDreamFloatySpaceBlock
using ..Ahorn, Maple

@mapdef Entity "CommunalHelper/DreamFloatySpaceBlock" DreamFloatySpaceBlock(x::Integer, y::Integer,
	width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
	featherMode::Bool = false, oneUse::Bool = false, doubleRefill::Bool=false, below::Bool=false)


const placements = Ahorn.PlacementDict(
    "Dream Floaty Space Block (Communal Helper)" => Ahorn.EntityPlacement(
        DreamFloatySpaceBlock,
		"rectangle"
    )
)

Ahorn.minimumSize(entity::DreamFloatySpaceBlock) = 8, 8
Ahorn.resizable(entity::DreamFloatySpaceBlock) = true, true

function Ahorn.selection(entity::DreamFloatySpaceBlock)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamFloatySpaceBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

	 Ahorn.Cairo.save(ctx)

    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1)

    fillColor = get(entity.data, "featherMode", false) ? (0.31, 0.69, 1.0, 0.4) : (0.0, 0.0, 0.0, 0.4)
	 lineColor = get(entity.data, "oneUse", false) ? (1.0, 0.0, 0.0, 1.0) : (1.0, 1.0, 1.0, 1.0)
    Ahorn.drawRectangle(ctx, 0, 0, width, height, fillColor, lineColor)

    Ahorn.restore(ctx)
end

end
